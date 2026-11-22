using Extenso.Data.Entity;

namespace Inferno.Tenants.Entities;

/// <summary>One site tenant. The current request is matched against <see cref="Hosts"/>.</summary>
public class Tenant : BaseEntity<int>
{
    /// <summary>The name shown for this tenant.</summary>
    public string Name { get; set; }

    /// <summary>The public address stored for this tenant.</summary>
    public string Url { get; set; }

    /// <summary>When <see langword="true"/>, generated links prefer <see cref="SecureUrl"/>.</summary>
    public bool SslEnabled { get; set; }

    /// <summary>The https address used for generated links when <see cref="SslEnabled"/> is <see langword="true"/>. May be <see langword="null"/>.</summary>
    public string SecureUrl { get; set; }

    /// <summary>Comma-separated host names that select this tenant. May be <see langword="null"/>.</summary>
    public string Hosts { get; set; }
}
