using Dependo;
using Inferno.Web.Mvc.Controllers;
using Inferno.Web.Mvc.Themes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Inferno.Web.Mvc.Razor;

/// <summary>
/// Adds view locations from the <see cref="ILocationFormatProvider"/> that lives in the controller's assembly.
/// A view name that contains a dot is also looked up as <c>/{name}.cshtml</c>.
/// </summary>
public class InfernoViewLocationExpander : IViewLocationExpander
{
    /// <summary>Providers resolved the first time an expander is constructed. Later instances reuse them.</summary>
    protected static IEnumerable<ILocationFormatProvider> locationFormatProviders;

    /// <summary>View, partial, and layout formats keyed by the provider assembly's full name.</summary>
    protected static Dictionary<string, List<string>> allViewLocationFormats;

    /// <summary>Area view, partial, and layout formats keyed by the provider assembly's full name.</summary>
    protected static Dictionary<string, List<string>> allAreaViewLocationFormats;

    /// <summary>Resolves the location providers once and caches their formats by assembly name.</summary>
    public InfernoViewLocationExpander()
    {
        if (locationFormatProviders == null)
        {
            locationFormatProviders = DependoResolver.Instance.ResolveAll<ILocationFormatProvider>() ?? Enumerable.Empty<ILocationFormatProvider>();
            allViewLocationFormats = locationFormatProviders.ToDictionary(k => k.GetType().Assembly.FullName, GetViewLocationFormats);
            allAreaViewLocationFormats = locationFormatProviders.ToDictionary(k => k.GetType().Assembly.FullName, GetAreaViewLocationFormats);
        }
    }

    /// <summary>Adds no values. The locations do not vary per request.</summary>
    /// <param name="context">The expander context. It is not read.</param>
    public virtual void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    /// <summary>
    /// Prepends <c>/{view name}.cshtml</c> when the view name contains a dot, then appends the formats
    /// registered for the controller's assembly.
    /// </summary>
    /// <param name="context">The view being located. Used for the view name and the controller type.</param>
    /// <param name="viewLocations">The locations already proposed.</param>
    /// <returns>The locations, with this expander's formats appended.</returns>
    public virtual IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.ViewName.Contains('.'))
        {
            viewLocations = new[]
            {
                "/" + context.ViewName + ".cshtml" // Embedded Views
            }
            .Concat(viewLocations);
        }

        string controllerAssemblyName = null;

        if (context.ActionContext.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
        {
            var controllerTypeInfo = controllerActionDescriptor.ControllerTypeInfo;
            controllerAssemblyName = controllerTypeInfo.Assembly.FullName;
        }
        else
        {
            var controllerContext = new ControllerContext(context.ActionContext);
            var factory = CreateControllerFactory();
            object controller = factory.CreateController(controllerContext);
            controllerAssemblyName = controller.GetType().Assembly.FullName;
        }

        // Remove everything except base (main app) paths and the plugin paths

        // TODO: Test
        if (!string.IsNullOrEmpty(controllerAssemblyName))
        {
            var validViewLocationFormats = allViewLocationFormats
                .Where(x => x.Key == controllerAssemblyName)
                .SelectMany(x => x.Value);

            var validAreaViewLocationFormats = allAreaViewLocationFormats
                .Where(x => x.Key == controllerAssemblyName)
                .SelectMany(x => x.Value);

            viewLocations = viewLocations
                .Concat(validViewLocationFormats)
                .Concat(validAreaViewLocationFormats);
        }

        return viewLocations;
    }

    private static List<string> GetViewLocationFormats(ILocationFormatProvider locationFormatProvider)
    {
        var formats = locationFormatProvider.ViewLocationFormats.ToList();
        formats.AddRange(locationFormatProvider.PartialViewLocationFormats);
        formats.AddRange(locationFormatProvider.MasterLocationFormats);
        return formats;
    }

    private static List<string> GetAreaViewLocationFormats(ILocationFormatProvider locationFormatProvider)
    {
        var formats = locationFormatProvider.AreaViewLocationFormats.ToList();
        formats.AddRange(locationFormatProvider.AreaPartialViewLocationFormats);
        formats.AddRange(locationFormatProvider.AreaMasterLocationFormats);
        return formats;
    }

    private static DefaultControllerFactory CreateControllerFactory() => new(
            new DefaultControllerActivator(new TypeActivatorCache()),
            new IControllerPropertyActivator[]
            {
                new DefaultControllerPropertyActivator(),
            });
}