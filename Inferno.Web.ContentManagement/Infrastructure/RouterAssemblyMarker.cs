using Inferno.Web.Infrastructure;

namespace Inferno.Web.ContentManagement.Infrastructure;

/// <summary>
/// Marks this assembly so MVC discovers its controllers and Razor components.
/// The marker is registered from <see cref="DependencyRegistrar"/>.
/// </summary>
public class RouterAssemblyMarker : IRouterAssemblyMarker
{
}