using Microsoft.AspNetCore.Routing;

namespace Inferno.Web.Mvc.Routing;

/// <summary>Contributes conventional routes and endpoints. This interface does not sort providers; a host may use <see cref="Priority"/> when it does.</summary>
public interface IRouteProvider
{
    /// <summary>Adds conventional routes to <paramref name="routes"/>.</summary>
    /// <param name="routes">The route builder.</param>
    void RegisterRoutes(IRouteBuilder routes);

    /// <summary>Adds endpoints to <paramref name="endpoints"/>.</summary>
    /// <param name="endpoints">The endpoint builder.</param>
    void RegisterEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>The order a host uses when more than one provider is registered. This interface does not sort them.</summary>
    int Priority { get; }
}