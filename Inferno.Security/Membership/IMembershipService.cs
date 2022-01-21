using System.Linq.Expressions;

namespace Inferno.Security.Membership
{
    //public enum UserStatus
    //{
    //    NotRegistered = 0,
    //    Unconfirmed = 1,
    //    Locked = 2,
    //    Active = 3
    //}

    public interface IMembershipService
    {
        bool SupportsRolePermissions { get; }

        Task<string> GenerateEmailConfirmationToken(object userId);

        Task ConfirmEmail(object userId, string token);

        #region Users

        //IQueryable<InfernoUser> GetAllUsersAsQueryable(DbContext context, int? tenantId);

        Task<IEnumerable<InfernoUser>> GetAllUsers(int? tenantId);

        Task<IEnumerable<InfernoUser>> GetUsers(int? tenantId, Expression<Func<InfernoUser, bool>> predicate);

        Task<InfernoUser> GetUserById(object userId);

        Task<InfernoUser> GetUserByEmail(int? tenantId, string email);

        Task<InfernoUser> GetUserByName(int? tenantId, string userName);

        Task<IEnumerable<InfernoRole>> GetRolesForUser(object userId);

        Task<bool> DeleteUser(object userId);

        Task InsertUser(InfernoUser user, string password);

        Task UpdateUser(InfernoUser user);

        Task AssignUserToRoles(int? tenantId, object userId, IEnumerable<object> roleIds);

        Task ChangePassword(object userId, string newPassword);

        Task<string> GetUserDisplayName(InfernoUser user);

        #endregion Users

        #region Roles

        Task<IEnumerable<InfernoRole>> GetAllRoles(int? tenantId);

        Task<InfernoRole> GetRoleById(object roleId);

        Task<IEnumerable<InfernoRole>> GetRolesByIds(IEnumerable<object> roleIds);

        Task<InfernoRole> GetRoleByName(int? tenantId, string roleName);

        Task<bool> DeleteRole(object roleId);

        Task InsertRole(InfernoRole role);

        Task UpdateRole(InfernoRole role);

        Task<IEnumerable<InfernoUser>> GetUsersByRoleId(object roleId);

        Task<IEnumerable<InfernoUser>> GetUsersByRoleName(int? tenantId, string roleName);

        #endregion Roles

        #region Permissions

        Task<IEnumerable<InfernoPermission>> GetAllPermissions(int? tenantId);

        Task<InfernoPermission> GetPermissionById(object permissionId);

        Task<InfernoPermission> GetPermissionByName(int? tenantId, string permissionName);

        Task<IEnumerable<InfernoPermission>> GetPermissionsForRole(int? tenantId, string roleName);

        Task AssignPermissionsToRole(object roleId, IEnumerable<object> permissionIds);

        Task<bool> DeletePermission(object permissionId);

        Task<bool> DeletePermissions(IEnumerable<object> permissionIds);

        Task InsertPermission(InfernoPermission permission);

        Task InsertPermissions(IEnumerable<InfernoPermission> permissions);

        Task UpdatePermission(InfernoPermission permission);

        #endregion Permissions

        #region Profile

        Task<IDictionary<string, string>> GetProfile(string userId);

        Task<IEnumerable<UserProfile>> GetProfiles(IEnumerable<string> userIds);

        Task UpdateProfile(string userId, IDictionary<string, string> profile, bool deleteExisting = false);

        Task<string> GetProfileEntry(string userId, string key);

        Task SaveProfileEntry(string userId, string key, string value);

        Task DeleteProfileEntry(string userId, string key);

        Task<IEnumerable<InfernoUserProfileEntry>> GetProfileEntriesByKey(int? tenantId, string key);

        Task<IEnumerable<InfernoUserProfileEntry>> GetProfileEntriesByKeyAndValue(int? tenantId, string key, string value);

        Task<bool> ProfileEntryExists(int? tenantId, string key, string value, string userId = null);

        #endregion Profile

        Task EnsureAdminRoleForTenant(int? tenantId);
    }

    public class UserProfile
    {
        public string UserId { get; set; }

        public IDictionary<string, string> Profile { get; set; }
    }
}