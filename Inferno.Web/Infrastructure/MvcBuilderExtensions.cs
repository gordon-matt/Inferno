using Dependo;
using Inferno.Helpers;
using Inferno.Plugins;
using Inferno.Plugins.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Inferno.Web.Infrastructure;

public static class MvcBuilderExtensions
{
    /// <summary>
    /// Discovers every assembly in the application's bin folder that contains an
    /// <see cref="IRouterAssemblyMarker"/> implementation and registers it as an MVC
    /// <see cref="Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPart"/>.
    ///
    /// This is the equivalent of MantleCMS's <c>AddMantleEmbeddedFileProviders</c>
    /// pattern. The Razor SDK normally emits an <c>[ApplicationPart]</c> attribute
    /// for class libraries that lets ASP.NET Core auto-discover them, but that
    /// discovery does not always kick in for transitively-referenced assemblies in
    /// this solution layout (and especially fails for plugins loaded at runtime).
    /// Driving discovery from <see cref="WebAppTypeFinder"/> guarantees that every
    /// controller-bearing assembly is wired into MVC, OData and Swagger.
    /// </summary>
    public static IMvcBuilder AddInfernoApplicationParts(this IMvcBuilder builder)
    {
        // WebAppTypeFinder eagerly loads every dll in the bin folder, which is
        // important here: at this point in startup AppDomain.CurrentDomain only
        // contains assemblies that have already been touched by user code.
        var typeFinder = new WebAppTypeFinder();

        var markerType = typeof(IRouterAssemblyMarker);

        var assemblies = typeFinder.GetAssemblies()
            .Where(a => SafeContainsMarker(a, markerType))
            .Distinct();

        foreach (var assembly in assemblies)
        {
            builder.AddApplicationPart(assembly);
        }

        return builder;
    }

    /// <summary>
    /// Discovers plugins under <c>~/Plugins</c>, shadow-copies their assemblies
    /// per the supplied <see cref="InfernoPluginOptions"/>, and registers each
    /// plugin assembly as an MVC <see cref="Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPart"/>.
    ///
    /// Call this AFTER <see cref="AddInfernoApplicationParts"/> so the host's
    /// own controllers are registered first - plugin parts are appended on top.
    /// </summary>
    /// <param name="builder">The MVC builder whose <c>PartManager</c> receives the plugin parts.</param>
    /// <param name="configuration">Used to bind the <c>InfernoPluginOptions</c> section.</param>
    public static IMvcBuilder AddInfernoPlugins(
        this IMvcBuilder builder,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        // PluginManager resolves "~/Plugins" via CommonHelper.MapPath, which
        // depends on CommonHelper.BaseDirectory being set. Default to the
        // host's content root - matches the way MantleCMS bootstraps plugins.
        if (string.IsNullOrEmpty(CommonHelper.BaseDirectory))
        {
            CommonHelper.BaseDirectory = hostEnvironment.ContentRootPath;
        }

        var options = new InfernoPluginOptions();
        configuration.GetSection(nameof(InfernoPluginOptions)).Bind(options);

        builder.Services.Configure<InfernoPluginOptions>(
            configuration.GetSection(nameof(InfernoPluginOptions)));

        // ApplicationPartManager.Initialize must run BEFORE the host is built
        // so that controllers / razor parts contributed by plugins are visible
        // to MVC's ActionDescriptorCollectionProvider.
        PluginManager.Initialize(builder.PartManager, options);

        return builder;
    }

    private static bool SafeContainsMarker(System.Reflection.Assembly assembly, Type markerType)
    {
        try
        {
            return assembly.ExportedTypes.Any(t =>
                !t.IsInterface &&
                !t.IsAbstract &&
                markerType.IsAssignableFrom(t));
        }
        catch
        {
            // Ignore assemblies that fail to enumerate (e.g. partially loaded,
            // unsigned, or referencing missing dependencies).
            return false;
        }
    }
}