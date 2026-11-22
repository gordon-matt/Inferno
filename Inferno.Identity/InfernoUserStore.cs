using System.Globalization;
using System.Security.Claims;
using Dependo;
using Inferno.Identity.Entities;
using Inferno.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Identity;

/// <summary>
/// An Identity user store for the current tenant. A request with no tenant sees host accounts only.
/// Sign-in still finds a host account when the current tenant has no user by that name, so the host
/// can open any site. Roles and new accounts stay in the current tenant: a tenant cannot be given a host role.
/// </summary>
/// <typeparam name="TUser">The user type.</typeparam>
/// <typeparam name="TRole">The role type.</typeparam>
/// <typeparam name="TContext">The database context.</typeparam>
public abstract class InfernoUserStore<TUser, TRole, TContext>
    : UserStore<TUser, TRole, TContext, string, IdentityUserClaim<string>, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
    where TUser : InfernoIdentityUser
    where TRole : InfernoIdentityRole
    where TContext : DbContext
{
    protected IWorkContext workContext;

    /// <summary>Creates a store on <paramref name="context"/>.</summary>
    /// <param name="context">The database context.</param>
    /// <param name="describer">Describes Identity errors. <see langword="null"/> lets the base store choose the default describer.</param>
    public InfernoUserStore(TContext context, IdentityErrorDescriber describer = null)
        : base(context, describer)
    {
    }

    #region Properties

    protected DbSet<TRole> Roles => Context.Set<TRole>();
    protected int? TenantId => CurrentTenantId.Resolve();
    protected DbSet<IdentityUserClaim<string>> UserClaims => Context.Set<IdentityUserClaim<string>>();
    protected DbSet<IdentityUserLogin<string>> UserLogins => Context.Set<IdentityUserLogin<string>>();
    protected DbSet<IdentityUserRole<string>> UserRoles => Context.Set<IdentityUserRole<string>>();
    protected DbSet<TUser> UsersSet => Context.Set<TUser>();
    protected DbSet<IdentityUserToken<string>> UserTokens => Context.Set<IdentityUserToken<string>>();

    protected IWorkContext WorkContext => workContext ??= DependoResolver.Instance.Resolve<IWorkContext>();

    #endregion Properties

    /// <summary>Users in the current tenant. A request with no tenant sees host accounts only.</summary>
    public override IQueryable<TUser> Users
    {
        get
        {
            int? tenantId = TenantId;
            return base.Users.Where(user => user.TenantId == tenantId);
        }
    }

    /// <summary>
    /// Stamps the user with the current tenant when this request has one, then creates the user.
    /// A request with no tenant keeps the tenant already set on the user, including a host account.
    /// </summary>
    /// <param name="user">The user to create.</param>
    /// <param name="cancellationToken">Cancels before the tenant is stamped and while the base store creates the user.</param>
    /// <returns>The result from the base store.</returns>
    public override Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (TenantId is not null)
        {
            user.TenantId = TenantId;
        }

        return base.CreateAsync(user, cancellationToken);
    }

    /// <summary>The user when they belong to this request's tenant, or the host account when this request has a tenant and no tenant user has that id.</summary>
    /// <param name="userId">The user id.</param>
    /// <param name="cancellationToken">Cancels the user query.</param>
    /// <returns>The user, or <see langword="null"/> when the id is missing or belongs to another tenant.</returns>
    public override async Task<TUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await base.FindByIdAsync(userId, cancellationToken);
        return Visible(user) ? user : null;
    }

    /// <summary>
    /// The external login's user when they belong to this request's tenant or are the host account.
    /// A login owned by another tenant is ignored, so signing in on one site cannot open another site's account.
    /// </summary>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="providerKey">The provider's user key.</param>
    /// <param name="cancellationToken">Cancels the login query.</param>
    /// <returns>The user, or <see langword="null"/> when the login is missing or belongs to another tenant.</returns>
    public override async Task<TUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();

        var login = await UserLogins.SingleOrDefaultAsync(
            userLogin => userLogin.LoginProvider == loginProvider && userLogin.ProviderKey == providerKey,
            cancellationToken);
        if (login is null)
        {
            return null;
        }

        var user = await UsersSet.FindAsync([login.UserId], cancellationToken);
        return Visible(user) ? user : null;
    }

    /// <summary>Adds <paramref name="user"/> to the current-tenant or host role named <paramref name="normalizedRoleName"/>.</summary>
    /// <param name="user">The user to add.</param>
    /// <param name="normalizedRoleName">The role's normalized name.</param>
    /// <param name="cancellationToken">Cancels before and while the role is loaded.</param>
    /// <returns>A task that completes when the membership row has been added. The caller saves the context.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="user"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="normalizedRoleName"/> is <see langword="null"/> or white space.</exception>
    /// <exception cref="InvalidOperationException">No current-tenant or host role has that normalized name.</exception>
    /// <exception cref="ObjectDisposedException">The store has been disposed.</exception>
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
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Role {0} does not exist in this tenant.", normalizedRoleName));
        }

        UserRoles.Add(CreateUserRole(user, roleEntity));
    }

    /// <summary>Finds a current-tenant or host user by normalized email.</summary>
    /// <param name="normalizedEmail">The normalized email address.</param>
    /// <param name="cancellationToken">Cancels the user query.</param>
    /// <returns>The matching user, or <see langword="null"/> when none matches.</returns>
    /// <exception cref="ObjectDisposedException">The store has been disposed.</exception>
    public override async Task<TUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var tenantUser = await base.Users.FirstOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail && user.TenantId == TenantId,
            cancellationToken);
        if (tenantUser is not null || TenantId is null)
        {
            return tenantUser;
        }

        return await base.Users.FirstOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail && user.TenantId == null,
            cancellationToken);
    }

    /// <summary>Finds a current-tenant or host user by normalized user name.</summary>
    /// <param name="normalizedUserName">The normalized user name.</param>
    /// <param name="cancellationToken">Cancels the user query.</param>
    /// <returns>The matching user, or <see langword="null"/> when none matches.</returns>
    /// <exception cref="ObjectDisposedException">The store has been disposed.</exception>
    public override async Task<TUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        var tenantUser = await base.Users.FirstOrDefaultAsync(
            user => user.NormalizedUserName == normalizedUserName && user.TenantId == TenantId,
            cancellationToken);
        if (tenantUser is not null || TenantId is null)
        {
            return tenantUser;
        }

        return await base.Users.FirstOrDefaultAsync(
            user => user.NormalizedUserName == normalizedUserName && user.TenantId == null,
            cancellationToken);
    }

    /// <summary>Reports whether <paramref name="user"/> belongs to the current-tenant or host role named <paramref name="normalizedRoleName"/>.</summary>
    /// <param name="user">The user to check.</param>
    /// <param name="normalizedRoleName">The role's normalized name.</param>
    /// <param name="cancellationToken">Cancels the role and membership queries.</param>
    /// <returns><see langword="true"/> when the membership row exists. <see langword="false"/> when the role or the membership is missing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="user"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="normalizedRoleName"/> is <see langword="null"/> or white space.</exception>
    /// <exception cref="ObjectDisposedException">The store has been disposed.</exception>
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

    /// <summary>Removes <paramref name="user"/> from the current-tenant or host role named <paramref name="normalizedRoleName"/>.</summary>
    /// <param name="user">The user to remove.</param>
    /// <param name="normalizedRoleName">The role's normalized name.</param>
    /// <param name="cancellationToken">Cancels the role and membership queries.</param>
    /// <returns>A task that completes when the membership row has been removed, or when the role or membership is already absent.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="user"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="normalizedRoleName"/> is <see langword="null"/> or white space.</exception>
    /// <exception cref="ObjectDisposedException">The store has been disposed.</exception>
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

    /// <summary>Role names the user holds in their own tenant. A host account's host roles still resolve on a tenant site.</summary>
    /// <param name="user">The user whose roles are read.</param>
    /// <param name="cancellationToken">Cancels the role query.</param>
    /// <returns>The role names.</returns>
    public override async Task<IList<string>> GetRolesAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(user);

        int? tenantId = user.TenantId;
        return await (
            from membership in UserRoles
            join role in Context.Set<TRole>() on membership.RoleId equals role.Id
            where membership.UserId == user.Id && role.TenantId == tenantId
            select role.Name).ToListAsync(cancellationToken);
    }

    /// <summary>A role in the current tenant. Host roles are not roles of a tenant.</summary>
    protected override Task<TRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        int? tenantId = TenantId;
        return Context.Set<TRole>().SingleOrDefaultAsync(
            role => role.NormalizedName == normalizedRoleName && role.TenantId == tenantId,
            cancellationToken);
    }

    /// <summary>A user in the current tenant, or the host account during a tenant request.</summary>
    private bool Visible(TUser user) =>
        user is not null && (user.TenantId == TenantId || (TenantId is not null && user.TenantId is null));

    protected override async Task<IdentityUserRole<string>> FindUserRoleAsync(string userId, string roleId, CancellationToken cancellationToken) =>
        await UserRoles.FindAsync(new object[] { userId, roleId }, cancellationToken).AsTask();
}