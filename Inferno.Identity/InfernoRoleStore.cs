using System.Security.Claims;
using Dependo;
using Inferno.Identity.Entities;
using Inferno.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Identity
{
    public abstract class InfernoRoleStore<TRole, TContext> : RoleStore<TRole, TContext, string, IdentityUserRole<string>, IdentityRoleClaim<string>>
        where TRole : InfernoIdentityRole
        where TContext : DbContext
    {
        private IWorkContext workContext;

        public InfernoRoleStore(TContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }

        #region Private Properties

        private IWorkContext WorkContext => workContext ??= EngineContext.Current.Resolve<IWorkContext>();

        private int TenantId => WorkContext.CurrentTenant.Id;

        #endregion Private Properties

        public override async Task<IdentityResult> CreateAsync(TRole role, CancellationToken cancellationToken = default)
        {
            return await base.CreateAsync(role, cancellationToken);
        }

        protected override IdentityRoleClaim<string> CreateRoleClaim(TRole role, Claim claim)
        {
            return new IdentityRoleClaim<string> { RoleId = role.Id, ClaimType = claim.Type, ClaimValue = claim.Value };
        }

        // Get by ID should not need to override.. onyl for getting by name
        //public override Task<TRole> FindByIdAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    ThrowIfDisposed();
        //    var roleId = ConvertIdFromString(id);
        //    return Roles.FirstOrDefaultAsync(
        //        u =>
        //            u.Id.Equals(roleId)
        //            && (u.TenantId == TenantId || (u.TenantId == null)),
        //        cancellationToken);
        //}

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
}