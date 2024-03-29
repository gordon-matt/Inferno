﻿using System.Linq.Expressions;
using Extenso.Collections;
using Extenso.Data.Entity;
using Inferno.Exceptions;
using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Web.Security.Membership;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InfernoCMS.Identity.Services
{
    public abstract class IdentityMembershipService : IMembershipService
    {
        private readonly IDbContextFactory contextFactory;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IRepository<UserProfileEntry> userProfileRepository;

        private static readonly Dictionary<string, List<InfernoRole>> cachedUserRoles;

        static IdentityMembershipService()
        {
            cachedUserRoles = new Dictionary<string, List<InfernoRole>>();
        }

        public IdentityMembershipService(
            IDbContextFactory contextFactory,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IRepository<UserProfileEntry> userProfileRepository)
        {
            this.contextFactory = contextFactory;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.userProfileRepository = userProfileRepository;
        }

        #region IMembershipService Members

        public async Task<string> GenerateEmailConfirmationTokenAsync(object userId)
        {
            string id = userId.ToString();
            var user = await userManager.FindByIdAsync(id);
            return await userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task ConfirmEmailAsync(object userId, string token)
        {
            string id = userId.ToString();
            var user = await userManager.FindByIdAsync(id);
            await userManager.ConfirmEmailAsync(user, token);
        }

        #region Users

        private static IQueryable<InfernoUser> GetAllUsersAsQueryable(DbContext context, int? tenantId)
        {
            IQueryable<ApplicationUser> query = context.Set<ApplicationUser>();

            if (tenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == tenantId);
            }
            else
            {
                query = query.Where(x => x.TenantId == null);
            }

            return query
                .Select(x => new InfernoUser
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    UserName = x.UserName,
                    Email = x.Email,
                    IsLockedOut = x.LockoutEnabled
                });
        }

        public async Task<IEnumerable<InfernoUser>> GetAllUsersAsync(int? tenantId)
        {
            using var context = contextFactory.GetContext();
            return await GetAllUsersAsQueryable(context, tenantId).ToHashSetAsync();
        }

        public async Task<IEnumerable<InfernoUser>> GetUsersAsync(int? tenantId, Expression<Func<InfernoUser, bool>> predicate)
        {
            using var context = contextFactory.GetContext();
            return await GetAllUsersAsQueryable(context, tenantId)
                .Where(predicate)
                .ToHashSetAsync();
        }

        public async Task<InfernoUser> GetUserByIdAsync(object userId)
        {
            using var context = contextFactory.GetContext();
            var user = context.Set<ApplicationUser>().Find(userId);

            if (user == null)
            {
                return null;
            }

            return await Task.FromResult(new InfernoUser
            {
                Id = user.Id,
                TenantId = user.TenantId,
                UserName = user.UserName,
                Email = user.Email,
                IsLockedOut = user.LockoutEnabled
            });
        }

        public async Task<InfernoUser> GetUserByEmailAsync(int? tenantId, string email)
        {
            ApplicationUser user;

            using var context = contextFactory.GetContext();
            if (tenantId.HasValue)
            {
                user = await context.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Email == email);
            }
            else
            {
                user = await context.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.TenantId == null && x.Email == email);
            }

            if (user == null)
            {
                return null;
            }

            return new InfernoUser
            {
                Id = user.Id,
                TenantId = user.TenantId,
                UserName = user.UserName,
                Email = user.Email,
                IsLockedOut = user.LockoutEnabled
            };
        }

        public async Task<InfernoUser> GetUserByNameAsync(int? tenantId, string userName)
        {
            ApplicationUser user;

            using var context = contextFactory.GetContext();
            if (tenantId.HasValue)
            {
                user = await context.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserName == userName);
            }
            else
            {
                user = await context.Set<ApplicationUser>().FirstOrDefaultAsync(x => x.TenantId == null && x.UserName == userName);
            }

            if (user == null)
            {
                return null;
            }

            return new InfernoUser
            {
                Id = user.Id,
                TenantId = user.TenantId,
                UserName = user.UserName,
                Email = user.Email,
                IsLockedOut = user.LockoutEnabled
            };
        }

        public async Task<IEnumerable<InfernoRole>> GetRolesForUserAsync(object userId)
        {
            string id = userId.ToString();
            if (cachedUserRoles.TryGetValue(id, out List<InfernoRole> value))
            {
                return value;
            }

            var user = await userManager.FindByIdAsync(id);
            var roleNames = await userManager.GetRolesAsync(user);

            var roles = new List<InfernoRole>();
            foreach (string roleName in roleNames)
            {
                var superRole = await GetRoleByNameAsync(null, roleName);
                if (superRole != null)
                {
                    roles.Add(new InfernoRole
                    {
                        Id = superRole.Id,
                        TenantId = null,
                        Name = superRole.Name
                    });
                }

                var tenantRole = await GetRoleByNameAsync(user.TenantId, roleName);
                if (tenantRole != null)
                {
                    roles.Add(new InfernoRole
                    {
                        Id = tenantRole.Id,
                        TenantId = null,
                        Name = tenantRole.Name
                    });
                }
            }

            cachedUserRoles.Add(id, roles);
            return roles;
        }

        public async Task<bool> DeleteUserAsync(object userId)
        {
            string id = userId.ToString();
            var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user != null)
            {
                var result = await userManager.DeleteAsync(user);
                return result.Succeeded;
            }

            return false;
        }

        public async Task InsertUserAsync(InfernoUser user, string password)
        {
            // Check for spaces in UserName above, because of this:
            // http://stackoverflow.com/questions/30078332/bug-in-asp-net-identitys-usermanager
            string userName = (user.UserName.Contains(' ') ? user.UserName.Replace(' ', '_') : user.UserName);

            var appUser = new ApplicationUser
            {
                TenantId = user.TenantId,
                UserName = userName,
                Email = user.Email,
                LockoutEnabled = user.IsLockedOut
            };

            var result = await userManager.CreateAsync(appUser, password);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
                throw new InfernoException(errorMessage);
            }
        }

        public async Task UpdateUserAsync(InfernoUser user)
        {
            string userId = user.Id.ToString();
            var existingUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user != null)
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.LockoutEnabled = user.IsLockedOut;
                var result = await userManager.UpdateAsync(existingUser);

                if (!result.Succeeded)
                {
                    string errorMessage = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
                    throw new InfernoException(errorMessage);
                }
            }
        }

        //public async Task AssignUserToRoles(object userId, IEnumerable<object> roleIds)
        //{
        //    string uId = userId.ToString();

        //    var ids = roleIds.Select(x => Convert.ToString(x));
        //    var roleNames = await roleManager.Roles.Where(x => ids.Contains(x.Id)).Select(x => x.Name).ToListAsync();

        //    var currentRoles = await userManager.GetRolesAsync(uId);

        //    var toDelete = currentRoles.Where(x => !roleNames.Contains(x));
        //    var toAdd = roleNames.Where(x => !currentRoles.Contains(x));

        //    if (toDelete.Any())
        //    {
        //        foreach (string roleName in toDelete)
        //        {
        //            var result = await userManager.RemoveFromRoleAsync(uId, roleName);

        //            if (!result.Succeeded)
        //            {
        //                string errorMessage = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
        //                throw new InfernoException(errorMessage);
        //            }
        //        }
        //        cachedUserRoles.Remove(uId);
        //    }

        //    if (toAdd.Any())
        //    {
        //        foreach (string roleName in toAdd)
        //        {
        //            var result = await userManager.AddToRoleAsync(uId, roleName);

        //            if (!result.Succeeded)
        //            {
        //                string errorMessage = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
        //                throw new InfernoException(errorMessage);
        //            }
        //        }
        //        cachedUserRoles.Remove(uId);
        //    }
        //}

        public async Task AssignUserToRolesAsync(int? tenantId, object userId, IEnumerable<object> roleIds)
        {
            string uId = userId.ToString();

            IQueryable<string> currentRoleIds;

            using (var context = contextFactory.GetContext())
            {
                if (tenantId.HasValue)
                {
                    currentRoleIds = from ur in context.Set<IdentityUserRole<string>>()
                                     join r in context.Set<ApplicationRole>() on ur.RoleId equals r.Id
                                     where r.TenantId == tenantId && ur.UserId == uId
                                     select ur.RoleId;
                }
                else
                {
                    currentRoleIds = from ur in context.Set<IdentityUserRole<string>>()
                                     join r in context.Set<ApplicationRole>() on ur.RoleId equals r.Id
                                     where r.TenantId == null && ur.UserId == uId
                                     select ur.RoleId;
                }

                var rIds = roleIds.ToListOf<string>();

                var toDelete = from ur in context.Set<IdentityUserRole<string>>()
                               join r in context.Set<ApplicationRole>() on ur.RoleId equals r.Id
                               where r.TenantId == tenantId
                               && ur.UserId == uId
                               && !rIds.Contains(ur.RoleId)
                               select ur;

                var toAdd = rIds.Where(x => !currentRoleIds.Contains(x)).Select(x => new IdentityUserRole<string>
                {
                    UserId = uId,
                    RoleId = x
                });

                if (toDelete.Any())
                {
                    context.Set<IdentityUserRole<string>>().RemoveRange(toDelete);
                }

                if (toAdd.Any())
                {
                    context.Set<IdentityUserRole<string>>().AddRange(toAdd);
                }

                await context.SaveChangesAsync();
            }

            cachedUserRoles.Remove(uId);
        }

        public async Task ChangePasswordAsync(object userId, string newPassword)
        {
            //TODO: This doesn't seem to be working very well; no errors, but can't login with the given password
            string id = userId.ToString();
            var user = await userManager.FindByIdAsync(id);
            var result = await userManager.RemovePasswordAsync(user);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
                throw new InfernoException(errorMessage);
            }

            result = await userManager.AddPasswordAsync(user, newPassword);

            if (!result.Succeeded)
            {
                string errorMessage = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
                throw new InfernoException(errorMessage);
            }
            //var user = userManager.FindById(id);
            //string passwordHash = userManager.PasswordHasher.HashPassword(newPassword);
            //userStore.SetPasswordHashAsync(user, passwordHash);
            //userManager.UpdateSecurityStamp(id);
        }

        public async Task<string> GetUserDisplayNameAsync(InfernoUser user)
        {
            var profile = await GetProfileAsync(user.Id);

            bool hasFamilyName = profile.ContainsKey(AccountUserProfileProvider.Fields.FamilyName);
            bool hasGivenNames = profile.ContainsKey(AccountUserProfileProvider.Fields.GivenNames);

            if (hasFamilyName && hasGivenNames)
            {
                string familyName = profile[AccountUserProfileProvider.Fields.FamilyName];
                string givenNames = profile[AccountUserProfileProvider.Fields.GivenNames];

                if (profile.TryGetValue(AccountUserProfileProvider.Fields.ShowFamilyNameFirst, out string value))
                {
                    bool showFamilyNameFirst = bool.Parse(value);

                    if (showFamilyNameFirst)
                    {
                        return $"{familyName} {givenNames}";
                    }
                    return $"{givenNames} {familyName}";
                }
                return $"{givenNames} {familyName}";
            }
            else if (hasFamilyName)
            {
                return profile[AccountUserProfileProvider.Fields.FamilyName];
            }
            else if (hasGivenNames)
            {
                return profile[AccountUserProfileProvider.Fields.GivenNames];
            }
            else
            {
                return user.UserName;
            }
        }

        #endregion Users

        #region Set<ApplicationRole>()

        public async Task<IEnumerable<InfernoRole>> GetAllRolesAsync(int? tenantId)
        {
            IQueryable<ApplicationRole> query = roleManager.Roles;

            if (tenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == tenantId);
            }
            else
            {
                query = query.Where(x => x.TenantId == null);
            }

            return await query
                .Select(x => new InfernoRole
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<InfernoRole> GetRoleByIdAsync(object roleId)
        {
            string id = roleId.ToString();
            var role = await roleManager.FindByIdAsync(id);

            return new InfernoRole
            {
                Id = role.Id,
                Name = role.Name
            };
        }

        public async Task<IEnumerable<InfernoRole>> GetRolesByIdsAsync(IEnumerable<object> roleIds)
        {
            var ids = roleIds.ToListOf<string>();
            var roles = new List<ApplicationRole>();

            foreach (string id in ids)
            {
                var role = await roleManager.FindByIdAsync(id);
                roles.Add(role);
            }

            return roles.Select(x => new InfernoRole
            {
                Id = x.Id,
                Name = x.Name
            });
        }

        public async Task<InfernoRole> GetRoleByNameAsync(int? tenantId, string roleName)
        {
            ApplicationRole role;

            if (tenantId.HasValue)
            {
                role = await roleManager.Roles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Name == roleName);
            }
            else
            {
                role = await roleManager.Roles.FirstOrDefaultAsync(x => x.TenantId == null && x.Name == roleName);
            }

            if (role == null)
            {
                return null;
            }

            return new InfernoRole
            {
                Id = role.Id,
                Name = role.Name
            };
        }

        public async Task<bool> DeleteRoleAsync(object roleId)
        {
            string id = roleId.ToString();
            var role = await roleManager.FindByIdAsync(id);

            if (role != null)
            {
                var result = await roleManager.DeleteAsync(role);
                return result.Succeeded;
            }

            return false;
        }

        public async Task InsertRoleAsync(InfernoRole role)
        {
            var result = await roleManager.CreateAsync(new ApplicationRole
            {
                TenantId = role.TenantId,
                Name = role.Name
            });

            if (!result.Succeeded)
            {
                string errorMessage = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
                throw new InfernoException(errorMessage);
            }
        }

        public async Task UpdateRoleAsync(InfernoRole role)
        {
            string id = role.Id.ToString();
            var existingRole = await roleManager.Roles.FirstOrDefaultAsync(x => x.Id == id);

            if (existingRole != null)
            {
                existingRole.Name = role.Name;
                var result = await roleManager.UpdateAsync(existingRole);

                if (!result.Succeeded)
                {
                    string errorMessage = string.Join(Environment.NewLine, result.Errors.Select(x => x.Description));
                    throw new InfernoException(errorMessage);
                }
            }
        }

        public async Task<IEnumerable<InfernoUser>> GetUsersByRoleIdAsync(object roleId)
        {
            string rId = roleId.ToString();

            // TODO: Include(x => x.Users) won't work. It should map to a junction table first (AspNetUserRoles) and then get the Users from that.
            //      userManager.GetUsersInRoleAsync(role.Name) // <-- probably need a custom UserManager (to take TenantId into account) and use this to get the users
            var role = await roleManager.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == rId);

            var userIds = role.Users.Select(x => x.Id).ToList();
            var users = await userManager.Users.Where(x => userIds.Contains(x.Id)).ToHashSetAsync();

            return users.Select(x => new InfernoUser
            {
                Id = x.Id,
                TenantId = x.TenantId,
                UserName = x.UserName,
                Email = x.Email,
                IsLockedOut = x.LockoutEnabled
            });
        }

        public async Task<IEnumerable<InfernoUser>> GetUsersByRoleNameAsync(int? tenantId, string roleName)
        {
            ApplicationRole role;

            if (tenantId.HasValue)
            {
                role = await roleManager.Roles
                    .Include(x => x.Users)
                    .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Name == roleName);
            }
            else
            {
                role = await roleManager.Roles
                    .Include(x => x.Users)
                    .FirstOrDefaultAsync(x => x.TenantId == null && x.Name == roleName);
            }

            var userIds = role.Users.Select(x => x.Id).ToList();
            var users = await userManager.Users.Where(x => userIds.Contains(x.Id)).ToHashSetAsync();

            return users.Select(x => new InfernoUser
            {
                Id = x.Id,
                TenantId = x.TenantId,
                UserName = x.UserName,
                Email = x.Email,
                IsLockedOut = x.LockoutEnabled
            });
        }

        #endregion Set<ApplicationRole>()

        #region Profile

        public async Task<IDictionary<string, string>> GetProfileAsync(string userId)
        {
            using var connection = userProfileRepository.OpenConnection();
            return await connection.Query(x => x.UserId == userId).ToDictionaryAsync(k => k.Key, v => v.Value);
        }

        public async Task<IEnumerable<UserProfile>> GetProfilesAsync(IEnumerable<string> userIds)
        {
            using var connection = userProfileRepository.OpenConnection();
            var entries = await connection.Query(x => userIds.Contains(x.UserId)).ToListAsync();
            return entries.GroupBy(x => x.UserId).Select(x => new UserProfile
            {
                UserId = x.Key,
                Profile = x.ToDictionary(k => k.Key, v => v.Value)
            });
        }

        public async Task UpdateProfileAsync(string userId, IDictionary<string, string> profile, bool deleteExisting = false)
        {
            List<UserProfileEntry> entries = null;
            using (var connection = userProfileRepository.OpenConnection())
            {
                entries = await connection.Query(x => x.UserId == userId).ToListAsync();
            }

            if (deleteExisting)
            {
                await userProfileRepository.DeleteAsync(entries);

                var toInsert = profile.Select(x => new UserProfileEntry
                {
                    UserId = userId,
                    Key = x.Key,
                    Value = x.Value
                }).ToList();

                await userProfileRepository.InsertAsync(toInsert);
            }
            else
            {
                var toUpdate = new List<UserProfileEntry>();
                var toInsert = new List<UserProfileEntry>();

                foreach (var keyValue in profile)
                {
                    var existing = entries.FirstOrDefault(x => x.Key == keyValue.Key);

                    if (existing != null)
                    {
                        existing.Value = keyValue.Value;
                        toUpdate.Add(existing);
                    }
                    else
                    {
                        toInsert.Add(new UserProfileEntry
                        {
                            UserId = userId,
                            Key = keyValue.Key,
                            Value = keyValue.Value
                        });
                    }
                }

                if (toUpdate.Any())
                {
                    await userProfileRepository.UpdateAsync(toUpdate);
                }

                if (toInsert.Any())
                {
                    await userProfileRepository.InsertAsync(toInsert);
                }
            }
        }

        public async Task<string> GetProfileEntryAsync(string userId, string key)
        {
            var entry = await userProfileRepository.FindOneAsync(x =>
                x.UserId == userId &&
                x.Key == key);

            if (entry != null)
            {
                return entry.Value;
            }

            return null;
        }

        public async Task SaveProfileEntryAsync(string userId, string key, string value)
        {
            var entry = await userProfileRepository.FindOneAsync(x =>
                x.UserId == userId &&
                x.Key == key);

            if (entry != null)
            {
                entry.Value = value;
                await userProfileRepository.UpdateAsync(entry);
            }
            else
            {
                await userProfileRepository.InsertAsync(new UserProfileEntry
                {
                    UserId = userId,
                    Key = key,
                    Value = value
                });
            }
        }

        public async Task DeleteProfileEntryAsync(string userId, string key)
        {
            var entry = await userProfileRepository.FindOneAsync(x =>
                x.UserId == userId &&
                x.Key == key);

            if (entry != null)
            {
                await userProfileRepository.DeleteAsync(entry);
            }
        }

        public async Task<IEnumerable<InfernoUserProfileEntry>> GetProfileEntriesByKeyAsync(int? tenantId, string key)
        {
            using var connection = userProfileRepository.OpenConnection();
            var query = connection.Query();

            if (tenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == tenantId && x.Key == key);
            }
            else
            {
                query = query.Where(x => x.TenantId == null && x.Key == key);
            }

            return (await query.ToHashSetAsync())
                .Select(x => new InfernoUserProfileEntry
                {
                    Id = x.Id.ToString(),
                    UserId = x.UserId,
                    Key = x.Key,
                    Value = x.Value
                });
        }

        public async Task<IEnumerable<InfernoUserProfileEntry>> GetProfileEntriesByKeyAndValueAsync(int? tenantId, string key, string value)
        {
            using var connection = userProfileRepository.OpenConnection();
            var query = connection.Query();

            if (tenantId.HasValue)
            {
                query = query.Where(x => x.TenantId == tenantId && x.Key == key && x.Value == value);
            }
            else
            {
                query = query.Where(x => x.TenantId == null && x.Key == key && x.Value == value);
            }

            return (await query.ToHashSetAsync())
                .Select(x => new InfernoUserProfileEntry
                {
                    Id = x.Id.ToString(),
                    UserId = x.UserId,
                    Key = x.Key,
                    Value = x.Value
                });
        }

        public async Task<bool> ProfileEntryExistsAsync(int? tenantId, string key, string value, string userId = null)
        {
            using var connection = userProfileRepository.OpenConnection();
            IQueryable<UserProfileEntry> query = null;

            if (tenantId.HasValue)
            {
                query = connection.Query(x => x.TenantId == tenantId && x.Key == key && x.Value == value);
            }
            else
            {
                query = connection.Query(x => x.TenantId == null && x.Key == key && x.Value == value);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(x => x.UserId == userId);
            }
            return await query.AnyAsync();
        }

        #endregion Profile

        public async Task EnsureAdminRoleForTenantAsync(int? tenantId)
        {
            var administratorsRole = await GetRoleByNameAsync(tenantId, InfernoSecurityConstants.Roles.Administrators);
            if (administratorsRole == null)
            {
                await InsertRoleAsync(new InfernoRole { TenantId = tenantId, Name = InfernoSecurityConstants.Roles.Administrators });
                administratorsRole = await GetRoleByNameAsync(tenantId, InfernoSecurityConstants.Roles.Administrators);

                if (administratorsRole != null)
                {
                    // Assign all super admin users (NULL TenantId) to this new admin role
                    var superAdminUsers = await GetUsersByRoleNameAsync(null, InfernoSecurityConstants.Roles.Administrators);
                    foreach (var user in superAdminUsers)
                    {
                        await AssignUserToRolesAsync(tenantId, user.Id, new[] { administratorsRole.Id });
                    }
                }
            }
        }

        #endregion IMembershipService Members
    }
}