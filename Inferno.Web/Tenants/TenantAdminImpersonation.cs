using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Tenants;

/// <summary>
/// Whether this scope should hide host-only access and show a tenant administrator's view.
/// The host account still signed in; only the permissions they are judged by change.
/// </summary>
public sealed class TenantAdminImpersonation
{
    /// <summary>Key for the flag stored on the HTTP request so a Blazor circuit can copy it.</summary>
    public const string ItemKey = "Inferno.ImpersonatingTenantAdmin";

    /// <summary><see langword="true"/> when the host administrator is looking at a tenant other than this host's own.</summary>
    public bool IsActive { get; set; }
}

/// <summary>Copies <see cref="TenantAdminImpersonation"/> onto the circuit when it opens.</summary>
/// <param name="impersonation">The circuit's copy of the flag.</param>
/// <param name="httpContextAccessor">The request that opened the circuit.</param>
public sealed class TenantAdminImpersonationCircuitHandler(
    TenantAdminImpersonation impersonation,
    IHttpContextAccessor httpContextAccessor) : CircuitHandler
{
    /// <inheritdoc />
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        impersonation.IsActive = httpContextAccessor.HttpContext?.Items[TenantAdminImpersonation.ItemKey] is true;
        return Task.CompletedTask;
    }
}
