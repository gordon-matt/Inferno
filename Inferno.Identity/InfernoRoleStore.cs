using System.Security.Claims;
using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Identity;

/// <summary>
/// An Identity role store limited to the current tenant and to host roles, whose <see cref="ITenantEntity.TenantId"/> is <see langword="null"/>.
/// A new role with no tenant is stamped with the current tenant.
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

    /// <summary>Roles in the current tenant, plus host roles.</summary>
    public override IQueryable<TRole> Roles
    {
        get
        {
            int? tenantId = TenantId;
            return base.Roles.Where(role => role.TenantId == tenantId || role.TenantId == null);
        }
    }

    /// <summary>Stamps a missing tenant with the current tenant, then creates the role.</summary>
    /// <param name="role">The role to create. A <see langword="null"/> <see cref="ITenantEntity.TenantId"/> is replaced with the current tenant.</param>
    /// <param name="cancellationToken">Cancels before the tenant is stamped and while the base store creates the role.</param>
    /// <returns>The result from the base store.</returns>
    public override Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        role.TenantId ??= TenantId;
        return base.CreateAsync(role, cancellationToken);
    }

    protected override IdentityRoleClaim<string> CreateRoleClaim(TRole role, Claim claim) => new() { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value };

    /// <summary>Finds a current-tenant or host role by normalized name.</summary>
    /// <param name="normalizedName">The normalized role name.</param>
    /// <param name="cancellationToken">Cancels the role query.</param>
    /// <returns>The matching role, or <see langword="null"/> when none matches.</returns>
    /// <exception cref="ObjectDisposedException">The store has been disposed.</exception>
    public override Task<TRole> FindByNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        return Roles.FirstOrDefaultAsync(
            r =>
                r.NormalizedName == normalizedName
                && (r.TenantId == TenantId || (r.TenantId == null)),
            cancellationToken);
    }
}