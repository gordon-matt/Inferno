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
/// An Identity user store limited to the current tenant and to host accounts, whose <see cref="ITenantEntity.TenantId"/> is <see langword="null"/>.
/// A new user with no tenant is stamped with the current tenant.
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

    /// <summary>Users in the current tenant, plus host accounts.</summary>
    public override IQueryable<TUser> Users
    {
        get
        {
            int? tenantId = TenantId;
            return base.Users.Where(user => user.TenantId == tenantId || user.TenantId == null);
        }
    }

    /// <summary>Stamps a missing tenant with the current tenant, then creates the user.</summary>
    /// <param name="user">The user to create. A <see langword="null"/> <see cref="ITenantEntity.TenantId"/> is replaced with the current tenant.</param>
    /// <param name="cancellationToken">Cancels before the tenant is stamped and while the base store creates the user.</param>
    /// <returns>The result from the base store.</returns>
    public override Task<IdentityResult> CreateAsync(TUser user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        user.TenantId ??= TenantId;
        return base.CreateAsync(user, cancellationToken);
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
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Role {0} does not exist.", normalizedRoleName));
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
        return await Users.FirstOrDefaultAsync(
            u =>
                u.NormalizedEmail == normalizedEmail
                && (u.TenantId == TenantId || (u.TenantId == null)),
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
        return await Users.FirstOrDefaultAsync(
            u =>
                u.NormalizedUserName == normalizedUserName
                && (u.TenantId == TenantId || (u.TenantId == null)),
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

    protected override async Task<TRole> FindRoleAsync(string normalizedRoleName, CancellationToken cancellationToken) =>
        await Roles.SingleOrDefaultAsync(r =>
            r.NormalizedName == normalizedRoleName
            && (r.TenantId == TenantId || (r.TenantId == null)),
            cancellationToken);

    protected override async Task<IdentityUserRole<string>> FindUserRoleAsync(string userId, string roleId, CancellationToken cancellationToken) =>
        await UserRoles.FindAsync(new object[] { userId, roleId }, cancellationToken).AsTask();
}