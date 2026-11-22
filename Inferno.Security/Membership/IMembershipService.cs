using System.Linq.Expressions;

namespace Inferno.Security.Membership;

/// <summary>
/// Users, roles, and profile values.
/// A <see langword="null"/> tenant id means the host scope, where <see cref="InfernoUser.TenantId"/> is <see langword="null"/>. It does not mean every tenant.
/// Identity failures are thrown as <see cref="Inferno.Exceptions.InfernoException"/> with the error descriptions joined by newlines.
/// </summary>
public interface IMembershipService
{
    /// <summary>Creates an email-confirmation token for <paramref name="userId"/>.</summary>
    /// <param name="userId">The user id. Its text form is the Identity id.</param>
    /// <returns>The confirmation token.</returns>
    Task<string> GenerateEmailConfirmationTokenAsync(object userId);

    /// <summary>Confirms the email for <paramref name="userId"/> with <paramref name="token"/>.</summary>
    /// <param name="userId">The user id. Its text form is the Identity id.</param>
    /// <param name="token">The token from <see cref="GenerateEmailConfirmationTokenAsync"/>.</param>
    Task ConfirmEmailAsync(object userId, string token);

    #region Users

    /// <summary>Returns the users in one tenant scope.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for host accounts.</param>
    /// <returns>The matching users. <see cref="InfernoUser.IsLockedOut"/> is Identity's lockout-enabled flag, and <see cref="InfernoUser.EmailConfirmed"/> is left <see langword="false"/>.</returns>
    Task<IEnumerable<InfernoUser>> GetAllUsersAsync(int? tenantId);

    /// <summary>Returns the users in one tenant scope that also match <paramref name="predicate"/>.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for host accounts.</param>
    /// <param name="predicate">A filter applied after the tenant filter.</param>
    /// <returns>The matching users, with the same fields filled as <see cref="GetAllUsersAsync"/>.</returns>
    Task<IEnumerable<InfernoUser>> GetUsersAsync(int? tenantId, Expression<Func<InfernoUser, bool>> predicate);

    /// <summary>Returns the user with <paramref name="userId"/>, in any tenant.</summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The user, or <see langword="null"/> when no account has that id.</returns>
    Task<InfernoUser> GetUserByIdAsync(object userId);

    /// <summary>Returns the user with <paramref name="email"/> in one tenant scope.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for a host account.</param>
    /// <param name="email">The email address, compared as stored.</param>
    /// <returns>The user, or <see langword="null"/> when none match.</returns>
    Task<InfernoUser> GetUserByEmailAsync(int? tenantId, string email);

    /// <summary>Returns the user with <paramref name="userName"/> in one tenant scope.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for a host account.</param>
    /// <param name="userName">The sign-in name, compared as stored.</param>
    /// <returns>The user, or <see langword="null"/> when none match.</returns>
    Task<InfernoUser> GetUserByNameAsync(int? tenantId, string userName);

    /// <summary>
    /// Returns the roles assigned to <paramref name="userId"/>.
    /// For each role name, a matching host role and a matching role in the user's tenant are both included.
    /// The returned objects have a null tenant id. The result is cached for the process until <see cref="AssignUserToRolesAsync"/> removes that user.
    /// </summary>
    /// <param name="userId">The user id. The user must exist.</param>
    /// <returns>The cached or freshly loaded roles.</returns>
    Task<IEnumerable<InfernoRole>> GetRolesForUserAsync(object userId);

    /// <summary>Deletes the user with <paramref name="userId"/>.</summary>
    /// <param name="userId">The user id.</param>
    /// <returns><see langword="false"/> when the user does not exist or Identity rejects the delete. Otherwise <see langword="true"/>.</returns>
    Task<bool> DeleteUserAsync(object userId);

    /// <summary>
    /// Creates an account from <paramref name="user"/>. A space in the user name is stored as an underscore.
    /// A null tenant is written back after create when the store stamps the current tenant instead.
    /// </summary>
    /// <param name="user">The account to create. <see cref="InfernoUser.EmailConfirmed"/> and <see cref="InfernoUser.IsLockedOut"/> are copied.</param>
    /// <param name="password">The initial password.</param>
    /// <exception cref="Inferno.Exceptions.InfernoException">Identity rejected the create, or the follow-up write that restores a null tenant.</exception>
    Task InsertUserAsync(InfernoUser user, string password);

    /// <summary>Updates the user name, email, and lockout-enabled flag of the account with <paramref name="user"/>'s id. The account must already exist.</summary>
    /// <param name="user">The new values. The id selects the account. Tenant and email-confirmed are not written.</param>
    /// <exception cref="Inferno.Exceptions.InfernoException">Identity rejected the update.</exception>
    Task UpdateUserAsync(InfernoUser user);

    /// <summary>
    /// Replaces the user's roles in one tenant scope with <paramref name="roleIds"/>.
    /// Every id must belong to that scope. The process-wide role cache for the user is dropped.
    /// </summary>
    /// <param name="tenantId">The tenant whose role links are replaced, or <see langword="null"/> for host roles.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="roleIds">The role ids the user should have in that scope. An empty list removes them all.</param>
    /// <exception cref="Inferno.Exceptions.InfernoException">A role id does not belong to <paramref name="tenantId"/>.</exception>
    Task AssignUserToRolesAsync(int? tenantId, object userId, IEnumerable<object> roleIds);

    /// <summary>Removes the current password and sets <paramref name="newPassword"/>.</summary>
    /// <param name="userId">The user id. The user must exist.</param>
    /// <param name="newPassword">The password to store.</param>
    /// <exception cref="Inferno.Exceptions.InfernoException">Identity rejected removing the old password or adding the new one.</exception>
    Task ChangePasswordAsync(object userId, string newPassword);

    /// <summary>
    /// Builds a display name from the profile. Family and given names are used together when both exist.
    /// <c>ShowFamilyNameFirst</c> set to <see langword="true"/> puts the family name first. One of those names is used when the other is missing. Otherwise the user name is returned.
    /// </summary>
    /// <param name="user">The user whose profile is read. <see cref="InfernoUser.Id"/> must be set.</param>
    /// <returns>The display name.</returns>
    /// <exception cref="FormatException"><c>ShowFamilyNameFirst</c> is present and is not a boolean.</exception>
    Task<string> GetUserDisplayNameAsync(InfernoUser user);

    #endregion Users

    #region Roles

    /// <summary>Returns the roles in one tenant scope, ordered by name. <see cref="InfernoRole.TenantId"/> is not copied onto the results.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for host roles.</param>
    /// <returns>The matching roles.</returns>
    Task<IEnumerable<InfernoRole>> GetAllRolesAsync(int? tenantId);

    /// <summary>Returns the role with <paramref name="roleId"/>. The role must exist. <see cref="InfernoRole.TenantId"/> is not copied.</summary>
    /// <param name="roleId">The role id.</param>
    /// <returns>The role.</returns>
    Task<InfernoRole> GetRoleByIdAsync(object roleId);

    /// <summary>Returns a role for each id. Each role must exist. <see cref="InfernoRole.TenantId"/> is not copied.</summary>
    /// <param name="roleIds">The role ids, in the order to return.</param>
    /// <returns>The roles.</returns>
    Task<IEnumerable<InfernoRole>> GetRolesByIdsAsync(IEnumerable<object> roleIds);

    /// <summary>Returns the role with <paramref name="roleName"/> in one tenant scope.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for a host role.</param>
    /// <param name="roleName">The role name, compared as stored.</param>
    /// <returns>The role, or <see langword="null"/> when none match. <see cref="InfernoRole.TenantId"/> is not copied.</returns>
    Task<InfernoRole> GetRoleByNameAsync(int? tenantId, string roleName);

    /// <summary>Deletes the role with <paramref name="roleId"/>.</summary>
    /// <param name="roleId">The role id.</param>
    /// <returns><see langword="false"/> when the role does not exist or Identity rejects the delete. Otherwise <see langword="true"/>.</returns>
    Task<bool> DeleteRoleAsync(object roleId);

    /// <summary>
    /// Creates <paramref name="role"/>. A null tenant is written back after create when the store stamps the current tenant instead.
    /// </summary>
    /// <param name="role">The role to create.</param>
    /// <exception cref="Inferno.Exceptions.InfernoException">Identity rejected the create, or the follow-up write that restores a null tenant.</exception>
    Task InsertRoleAsync(InfernoRole role);

    /// <summary>Updates the name of the role with <paramref name="role"/>'s id. Does nothing when that role is missing.</summary>
    /// <param name="role">The new name. The id selects the role.</param>
    /// <exception cref="Inferno.Exceptions.InfernoException">Identity rejected the update.</exception>
    Task UpdateRoleAsync(InfernoRole role);

    /// <summary>Returns the users linked to <paramref name="roleId"/>, in any tenant.</summary>
    /// <param name="roleId">The role id.</param>
    /// <returns>The users, with the same fields filled as <see cref="GetAllUsersAsync"/>.</returns>
    Task<IEnumerable<InfernoUser>> GetUsersByRoleIdAsync(object roleId);

    /// <summary>Returns the users linked to a role named <paramref name="roleName"/> in one tenant scope.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for the host role of that name.</param>
    /// <param name="roleName">The role name, compared as stored.</param>
    /// <returns>The users, with the same fields filled as <see cref="GetAllUsersAsync"/>.</returns>
    Task<IEnumerable<InfernoUser>> GetUsersByRoleNameAsync(int? tenantId, string roleName);

    #endregion Roles

    #region Profile

    /// <summary>Returns every profile entry for <paramref name="userId"/>.</summary>
    /// <param name="userId">The user id.</param>
    /// <returns>The entries keyed by field name.</returns>
    Task<IDictionary<string, string>> GetProfileAsync(string userId);

    /// <summary>Returns the profile entries for each of <paramref name="userIds"/>, grouped by user. A user with no entries is omitted.</summary>
    /// <param name="userIds">The user ids to load.</param>
    /// <returns>One profile per user that has at least one entry.</returns>
    Task<IEnumerable<UserProfile>> GetProfilesAsync(IEnumerable<string> userIds);

    /// <summary>
    /// Writes <paramref name="profile"/> for <paramref name="userId"/>.
    /// When <paramref name="deleteExisting"/> is <see langword="true"/>, the user's current entries are deleted first and only the dictionary remains.
    /// Otherwise matching keys are updated, new keys are inserted, and keys absent from the dictionary are left in place.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <param name="profile">The field names and values to write.</param>
    /// <param name="deleteExisting"><see langword="true"/> to replace the whole profile. <see langword="false"/> to merge.</param>
    Task UpdateProfileAsync(string userId, IDictionary<string, string> profile, bool deleteExisting = false);

    /// <summary>Returns one profile value.</summary>
    /// <param name="userId">The user id.</param>
    /// <param name="key">The field name.</param>
    /// <returns>The stored value, or <see langword="null"/> when the entry is missing.</returns>
    Task<string> GetProfileEntryAsync(string userId, string key);

    /// <summary>Inserts or updates one profile value.</summary>
    /// <param name="userId">The user id.</param>
    /// <param name="key">The field name.</param>
    /// <param name="value">The value to store.</param>
    Task SaveProfileEntryAsync(string userId, string key, string value);

    /// <summary>Deletes one profile value. Does nothing when the entry is missing.</summary>
    /// <param name="userId">The user id.</param>
    /// <param name="key">The field name.</param>
    Task DeleteProfileEntryAsync(string userId, string key);

    /// <summary>Returns profile entries with <paramref name="key"/> in one tenant scope.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for host entries.</param>
    /// <param name="key">The field name.</param>
    /// <returns>The matching entries.</returns>
    Task<IEnumerable<InfernoUserProfileEntry>> GetProfileEntriesByKeyAsync(int? tenantId, string key);

    /// <summary>Returns profile entries with <paramref name="key"/> and <paramref name="value"/> in one tenant scope.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for host entries.</param>
    /// <param name="key">The field name.</param>
    /// <param name="value">The value, compared as stored.</param>
    /// <returns>The matching entries.</returns>
    Task<IEnumerable<InfernoUserProfileEntry>> GetProfileEntriesByKeyAndValueAsync(int? tenantId, string key, string value);

    /// <summary>Reports whether a profile entry with <paramref name="key"/> and <paramref name="value"/> exists in one tenant scope.</summary>
    /// <param name="tenantId">The tenant, or <see langword="null"/> for host entries.</param>
    /// <param name="key">The field name.</param>
    /// <param name="value">The value, compared as stored.</param>
    /// <param name="userId">When set, only that user's entries count. <see langword="null"/> or empty counts every user in the scope.</param>
    /// <returns><see langword="true"/> when a matching entry exists.</returns>
    Task<bool> ProfileEntryExistsAsync(int? tenantId, string key, string value, string userId = null);

    #endregion Profile

    /// <summary>
    /// Creates the Administrators role for <paramref name="tenantId"/> when neither that scope nor the host already has one.
    /// A host Administrators role is left as the shared role and no tenant copy is created.
    /// When a new role is created, every host administrator is assigned to it.
    /// </summary>
    /// <param name="tenantId">The tenant that should have an Administrators role, or <see langword="null"/> for the host.</param>
    Task EnsureAdminRoleForTenantAsync(int? tenantId);
}

/// <summary>One user's profile entries.</summary>
public class UserProfile
{
    /// <summary>The user these entries belong to.</summary>
    public string UserId { get; set; }

    /// <summary>The field names and values.</summary>
    public IDictionary<string, string> Profile { get; set; }
}
