using System.Runtime.Serialization;
using Inferno.Tenants.Entities;
using Microsoft.AspNetCore.Identity;

namespace Inferno.Identity.Entities;

/// <summary>An Identity user that belongs to one tenant, or to the host when <see cref="ITenantEntity.TenantId"/> is <see langword="null"/>.</summary>
public abstract class InfernoIdentityUser : IdentityUser, ITenantEntity
{
    /// <inheritdoc />
    public int? TenantId { get; set; }

    #region IEntity Members

    /// <inheritdoc />
    [IgnoreDataMember] // OData v8 does not like this property and will break if we don't use [IgnoreDataMember] here.
    public object[] KeyValues => new object[] { Id };

    #endregion IEntity Members
}