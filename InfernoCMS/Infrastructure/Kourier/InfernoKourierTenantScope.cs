using System.Globalization;
using Inferno.Identity;
using Kourier.Core;

namespace InfernoCMS.Infrastructure.Kourier;

/// <summary>The Flatmate tenant for this request or circuit, as Kourier's tenant id.</summary>
public sealed class InfernoKourierTenantScope(IServiceProvider services) : IKourierTenantScope
{
    /// <inheritdoc />
    public string? TenantId
    {
        get
        {
            int? id = CurrentTenantId.Resolve(services);
            return id?.ToString(CultureInfo.InvariantCulture);
        }
    }
}