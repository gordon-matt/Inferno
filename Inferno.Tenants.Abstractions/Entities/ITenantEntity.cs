using Extenso.Data.Entity;

namespace Inferno.Tenants.Entities;

/// <summary>A row that belongs to one tenant. A null <see cref="TenantId"/> is the host scope.</summary>
public interface ITenantEntity : IEntity
{
    /// <summary>The owning tenant. <see langword="null"/> is a host row.</summary>
    int? TenantId { get; set; }
}

/// <summary>Base entity with a tenant id.</summary>
/// <typeparam name="T">The primary-key type.</typeparam>
public class TenantEntity<T> : BaseEntity<T>, ITenantEntity
{
    /// <inheritdoc />
    public int? TenantId { get; set; }
}
