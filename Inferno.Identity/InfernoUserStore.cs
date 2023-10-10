using Dependo;
using Inferno.Identity.Entities;
using Inferno.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Inferno.Identity
{
    public abstract class InfernoUserStore<TUser, TRole, TContext>
        : UserStore<TUser, TRole, TContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
        where TUser : InfernoIdentityUser
        where TRole : InfernoIdentityRole
        where TContext : DbContext
    {
        protected IWorkContext workContext;

        public InfernoUserStore(TContext context, IdentityErrorDescriber describer = null)
            : base(context, describer)
        {
        }

        #region Properties

        protected DbSet<TRole> Roles => Context.Set<TRole>();
        protected int TenantId => WorkContext.CurrentTenant.Id;
        protected DbSet<IdentityUserClaim<string>> UserClaims => Context.Set<IdentityUserClaim<string>>();
        protected DbSet<IdentityUserLogin<string>> UserLogins => Context.Set<IdentityUserLogin<string>>();
        protected DbSet<IdentityUserRole<string>> UserRoles => Context.Set<IdentityUserRole<string>>();
        protected DbSet<TUser> UsersSet => Context.Set<TUser>();
        protected DbSet<IdentityUserToken<string>> UserTokens => Context.Set<IdentityUserToken<string>>();

        protected IWorkContext WorkContext => workContext ??= EngineContext.Current.Resolve<IWorkContext>();

        #endregion Properties

        public override async Task AddToRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(normalizedRoleName));
            }

            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (roleEntity == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Role {0} does not exist.", normalizedRoleName));
            }

            UserRoles.Add(CreateUserRole(user, roleEntity));
        }

        public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return await Users.FirstOrDefaultAsync(
                u =>
                    u.NormalizedEmail == normalizedEmail
                    && (u.TenantId == TenantId || (u.TenantId == null)),
                cancellationToken);
        }

        public override async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return await Users.FirstOrDefaultAsync(
                u =>
                    u.NormalizedUserName == normalizedUserName
                    && (u.TenantId == TenantId || (u.TenantId == null)),
                cancellationToken);
        }

        public override async Task<bool> IsInRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(normalizedRoleName));
            }

            var role = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (role != null)
            {
                var userRole = await FindUserRoleAsync(user.Id, role.Id, cancellationToken);
                return userRole != null;
            }
            return false;
        }

        public override async Task RemoveFromRoleAsync(TUser user, string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException("Value cannot be null or empty", nameof(normalizedRoleName));
            }

            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (roleEntity != null)
            {
                var userRole = await FindUserRoleAsync(user.Id, roleEntity.Id, cancellationToken);
                if (userRole != null)
                {
                    UserRoles.Remove(userRole);
                }
            }
        }

        protected override IdentityUserClaim<string> CreateUserClaim(TUser user, Claim claim)
        {
            var userClaim = new IdentityUserClaim<string> { UserId = user.Id };
            userClaim.InitializeFromClaim(claim);
            return userClaim;
        }

        protected override IdentityUserLogin<string> CreateUserLogin(TUser user, UserLoginInfo login) =>
            new()
            {
                UserId = user.Id,
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider,
                ProviderDisplayName = login.ProviderDisplayName
            };

        protected override IdentityUserRole<string> CreateUserRole(TUser user, TRole role) =>
            new()
            {
                UserId = user.Id,
                RoleId = role.Id
            };

        protected override IdentityUserToken<string> CreateUserToken(TUser user, string loginProvider, string name, string value) =>
            new()
            {
                UserId = user.Id,
                LoginProvider = loginProvider,
                Name = name,
                Value = value
            };

        protected override async Task<TRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
            await Roles.SingleOrDefaultAsync(r =>
                r.NormalizedName == normalizedRoleName
                && (r.TenantId == TenantId || (r.TenantId == null)),
                cancellationToken);

        protected override async Task<IdentityUserRole<string>> FindUserRoleAsync(string userId, string roleId, CancellationToken cancellationToken) =>
            await UserRoles.FindAsync(new object[] { userId, roleId }, cancellationToken).AsTask();
    }
}