using System.Linq.Expressions;

namespace Inferno.Security.Membership
{
    public interface IMembershipService
    {
        Task<string> GenerateEmailConfirmationTokenAsync(object userId);

        Task ConfirmEmailAsync(object userId, string token);

        #region Users

        Task<IEnumerable<InfernoUser>> GetAllUsersAsync(int? tenantId);

        Task<IEnumerable<InfernoUser>> GetUsersAsync(int? tenantId, Expression<Func<InfernoUser, bool>> predicate);

        Task<InfernoUser> GetUserByIdAsync(object userId);

        Task<InfernoUser> GetUserByEmailAsync(int? tenantId, string email);

        Task<InfernoUser> GetUserByNameAsync(int? tenantId, string userName);

        Task<IEnumerable<InfernoRole>> GetRolesForUserAsync(object userId);

        Task<bool> DeleteUserAsync(object userId);

        Task InsertUserAsync(InfernoUser user, string password);

        Task UpdateUserAsync(InfernoUser user);

        Task AssignUserToRolesAsync(int? tenantId, object userId, IEnumerable<object> roleIds);

        Task ChangePasswordAsync(object userId, string newPassword);

        Task<string> GetUserDisplayNameAsync(InfernoUser user);

        #endregion Users

        #region Roles

        Task<IEnumerable<InfernoRole>> GetAllRolesAsync(int? tenantId);

        Task<InfernoRole> GetRoleByIdAsync(object roleId);

        Task<IEnumerable<InfernoRole>> GetRolesByIdsAsync(IEnumerable<object> roleIds);

        Task<InfernoRole> GetRoleByNameAsync(int? tenantId, string roleName);

        Task<bool> DeleteRoleAsync(object roleId);

        Task InsertRoleAsync(InfernoRole role);

        Task UpdateRoleAsync(InfernoRole role);

        Task<IEnumerable<InfernoUser>> GetUsersByRoleIdAsync(object roleId);

        Task<IEnumerable<InfernoUser>> GetUsersByRoleNameAsync(int? tenantId, string roleName);

        #endregion Roles

        #region Profile

        Task<IDictionary<string, string>> GetProfileAsync(string userId);

        Task<IEnumerable<UserProfile>> GetProfilesAsync(IEnumerable<string> userIds);

        Task UpdateProfileAsync(string userId, IDictionary<string, string> profile, bool deleteExisting = false);

        Task<string> GetProfileEntryAsync(string userId, string key);

        Task SaveProfileEntryAsync(string userId, string key, string value);

        Task DeleteProfileEntryAsync(string userId, string key);

        Task<IEnumerable<InfernoUserProfileEntry>> GetProfileEntriesByKeyAsync(int? tenantId, string key);

        Task<IEnumerable<InfernoUserProfileEntry>> GetProfileEntriesByKeyAndValueAsync(int? tenantId, string key, string value);

        Task<bool> ProfileEntryExistsAsync(int? tenantId, string key, string value, string userId = null);

        #endregion Profile

        Task EnsureAdminRoleForTenantAsync(int? tenantId);
    }

    public class UserProfile
    {
        public string UserId { get; set; }

        public IDictionary<string, string> Profile { get; set; }
    }
}