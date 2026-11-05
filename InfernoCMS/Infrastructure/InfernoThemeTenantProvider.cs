using System.Globalization;
using Inferno.Identity;
using Kameleon.Tenancy;

namespace InfernoCMS.Infrastructure;

/// <summary>Kameleon keys each site's theme settings by the Inferno tenant, stored as text.</summary>
public sealed class InfernoThemeTenantProvider : IThemeTenantProvider
{
    public string GetTenantId() => CurrentTenantId.Resolve().ToString(CultureInfo.InvariantCulture);
}
