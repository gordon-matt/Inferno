using System.Security.Claims;
using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Identity;

/// <summary>
/// An Identity role store for the current tenant. A request with no tenant sees host roles only.
/// A host role is not a role of a tenant, so a tenant cannot be assigned the host Administrators role.
/// </summary>
/// <typeparam name="TRole">The role type.</typeparam>
/// <typeparam name="TContext">The database context.</typeparam>
public abstract class InfernoRoleStore<TRole, TContext> : RoleStore<TRole, TContext, string, IdentityUserRole<string>, IdentityRoleClaim<string>>
    where TRole : InfernoIdentityRole
    where TContext : DbContext
{
    /// <summary>Creates a store on <paramref name="context"/>.</summary>
    /// <param name="context">The database context.</param>
    /// <param name="describer">Describes Identity errors. <see langword="null"/> lets the base store choose the default describer.</param>
    public InfernoRoleStore(TContext context, IdentityErrorDescriber describer = null)
        : base(context, describer)
    {
    }

    private int? TenantId => CurrentTenantId.Resolve();

    /// <summary>Roles in the current tenant. A request with no tenant sees host roles only.</summary>
    public override IQueryable<TRole> Roles
    {
        get
        {
            int? tenantId = TenantId;
            return base.Roles.Where(role => role.TenantId == tenantId);
        }
    }

    /// <summary>
    /// Stamps the role with the current tenant when this request has one, then creates the role.
    /// A request with no tenant keeps the tenant already set on the role, including a host role.
    /// </summary>
    /// <param name="role">The role to create.</param>
    /// <param name="cancellationToken">Cancels before the tenant is stamped and while the base store creates the role.</param>
    /// <returns>The result from the base store.</returns>
    public override Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (TenantId is not null)
        {
            role.TenantId = TenantId;
        }

        return base.CreateAsync(role, cancellationToken);
    }

    /// <summary>The role when it belongs to this request's tenant.</summary>
    /// <param name="id">The role id.</param>
    /// <param name="cancellationToken">Cancels the role query.</param>
    /// <returns>The role, or <see langword="null"/> when the id is missing or belongs to another tenant.</returns>
    public override async Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await base.FindByIdAsync(id, cancellationToken);
        return role is not null && role.TenantId == TenantId ? role : null;
    }

    protected override IdentityRoleClaim<string> CreateRoleClaim(TRole role, Claim claim) => new() { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value };

    /// <summary>Finds a role in the current tenant by normalized name. A host role is not returned during a tenant request.</summary>
    /// <param name="normalizedName">The normalized role name.</param>
    /// <param name="cancellationToken">Cancels the role query.</param>
    /// <returns>The matching role, or <see langword="null"/> when none matches.</returns>
    /// <exception cref="ObjectDisposedException">The store has been disposed.</exception>
    public override Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        int? tenantId = TenantId;
        return Roles.FirstOrDefaultAsync(
            role => role.NormalizedName == normalizedName && role.TenantId == tenantId,
            cancellationToken);
    }
}