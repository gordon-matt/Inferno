using System.Runtime.Serialization;
using Inferno.Tenants.Entities;
using Microsoft.AspNetCore.Identity;

namespace Inferno.Identity.Entities;

/// <summary>An Identity role that belongs to one tenant, or to the host when <see cref="ITenantEntity.TenantId"/> is <see langword="null"/>.</summary>
public abstract class InfernoIdentityRole : IdentityRole, ITenantEntity
{
    /// <summary>Creates a role with no name.</summary>
    public InfernoIdentityRole()
        : base()
    {
    }

    /// <summary>Creates a role named <paramref name="roleName"/>.</summary>
    /// <param name="roleName">The role name.</param>
    public InfernoIdentityRole(string roleName)
        : base(roleName)
    {
    }

    /// <inheritdoc />
    public int? TenantId { get; set; }

    #region IEntity Members

    /// <inheritdoc />
    [IgnoreDataMember] // OData v8 does not like this property and will break if we don't use [IgnoreDataMember] here.
    public object[] KeyValues => new object[] { Id };

    #endregion IEntity Members
}