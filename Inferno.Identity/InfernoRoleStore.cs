using System.Security.Claims;
using Inferno.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Identity;

public abstract class InfernoRoleStore<TRole, TContext> : RoleStore<TRole, TContext, string, IdentityUserRole<string>, IdentityRoleClaim<string>>
    where TRole : InfernoIdentityRole
    where TContext : DbContext
{
    public InfernoRoleStore(TContext context, IdentityErrorDescriber describer = null)
        : base(context, describer)
    {
    }

    private int TenantId => CurrentTenantId.Resolve();

    public override IQueryable<TRole> Roles
    {
        get
        {
            int tenantId = TenantId;
            return base.Roles.Where(role => role.TenantId == tenantId || role.TenantId == null);
        }
    }

    public override Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        role.TenantId ??= TenantId;
        return base.CreateAsync(role, cancellationToken);
    }

    protected override IdentityRoleClaim<string> CreateRoleClaim(TRole role, Claim claim) => new() { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value };

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