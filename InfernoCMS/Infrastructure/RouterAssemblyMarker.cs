using Inferno.Web.Infrastructure;

namespace InfernoCMS.Infrastructure;

/// <summary>
/// Marks this assembly for the Blazor router. Startup drops this host assembly from the additional-assembly list because it is already mapped.
/// </summary>
public class RouterAssemblyMarker : IRouterAssemblyMarker
{
}