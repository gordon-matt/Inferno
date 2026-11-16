using Dependo;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.Infrastructure;

public static class MvcBuilderExtensions
{
    /// <summary>
    /// Discovers every assembly that contains an <see cref="IRouterAssemblyMarker"/> and registers
    /// it as an MVC <see cref="Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPart"/>.
    ///
    /// The marker list comes from the Dependo catalog when the host was compiled with it, which
    /// avoids loading every DLL in the output folder. Plugin assemblies are added separately.
    /// </summary>
    public static IMvcBuilder AddInfernoApplicationParts(this IMvcBuilder builder)
    {
        var typeFinder = new WebAppTypeFinder();
        var assemblies = typeFinder.FindClassesOfType<IRouterAssemblyMarker>()
            .Select(type => type.Assembly)
            .Distinct();

        foreach (var assembly in assemblies)
        {
            builder.AddApplicationPart(assembly);
        }

        return builder;
    }
}