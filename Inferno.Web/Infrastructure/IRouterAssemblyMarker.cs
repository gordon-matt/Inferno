namespace Inferno.Web.Infrastructure;

/// <summary>Marks an assembly that <see cref="MvcBuilderExtensions.AddInfernoApplicationParts"/> registers as an MVC application part.</summary>
public interface IRouterAssemblyMarker
{
}

/// <inheritdoc cref="IRouterAssemblyMarker" />
public class RouterAssemblyMarker : IRouterAssemblyMarker
{
}