using Dependo;
using Extenso;
using Extenso.Collections;
using Extenso.Data.Entity;
using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Inferno.Web.Configuration;
using Inferno.Web.Configuration.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inferno.Web.Infrastructure
{
    /// <summary>
    /// Interface which should be implemented by tasks run on startup
    /// </summary>
    public interface IStartupTask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        Task ExecuteAsync();

        /// <summary>
        /// Gets order of this startup task implementation
        /// </summary>
        int Order { get; }
    }

    public class StartupTask : IStartupTask
    {
        #region IStartupTask Members

        public async Task ExecuteAsync()
        {
            await EnsureTenantAsync();

            var tenantService = EngineContext.Current.Resolve<ITenantService>();
            IEnumerable<int> tenantIds = null;

            using (var connection = tenantService.OpenConnection())
            {
                tenantIds = await connection.Query().Select(x => x.Id).ToListAsync();
            }

            var membershipService = EngineContext.Current.Resolve<IMembershipService>();
            await EnsureMembershipAsync(membershipService, tenantIds);

            await EnsureSettingsAsync(tenantIds);
        }

        public int Order => 1;

        #endregion IStartupTask Members

        private static async Task EnsureTenantAsync()
        {
            var tenantService = EngineContext.Current.Resolve<ITenantService>();

            if (await tenantService.CountAsync() == 0)
            {
                await tenantService.InsertAsync(new Tenant
                {
                    Name = "Default",
                    Url = "my-domain.com",
                    Hosts = "my-domain.com"
                });
            }
        }

        private static async Task EnsureMembershipAsync(IMembershipService membershipService, IEnumerable<int> tenantIds)
        {
            // We only run this method to ensure that the admin user has been setup as part of the installation process.
            //  If there are any users already in the DB...
            if ((await membershipService.GetAllUsersAsync(null)).Any())
            {
                // ... we assume the admin user is one of them. No need for further querying...
                return;
            }

            var dataSettings = EngineContext.Current.Resolve<DataSettings>();

            var adminUser = await membershipService.GetUserByEmailAsync(null, dataSettings.AdminEmail);
            if (adminUser == null)
            {
                await membershipService.InsertUserAsync(
                    new InfernoUser
                    {
                        TenantId = null,
                        UserName = dataSettings.AdminEmail,
                        Email = dataSettings.AdminEmail
                    },
                    dataSettings.AdminPassword);

                adminUser = await membershipService.GetUserByEmailAsync(null, dataSettings.AdminEmail);
                if (adminUser != null)
                {
                    // TODO: This doesn't work. Gets error like "No owin.Environment item was found in the context."
                    //// Confirm User
                    //string token = await membershipService.GenerateEmailConfirmationToken(adminUser.Id);
                    //await membershipService.ConfirmEmail(adminUser.Id, token);

                    var administratorsRole = await membershipService.GetRoleByNameAsync(null, InfernoSecurityConstants.Roles.Administrators);
                    if (administratorsRole == null)
                    {
                        await membershipService.InsertRoleAsync(new InfernoRole
                        {
                            TenantId = null,
                            Name = InfernoSecurityConstants.Roles.Administrators
                        });
                        administratorsRole = await membershipService.GetRoleByNameAsync(null, InfernoSecurityConstants.Roles.Administrators);
                        await membershipService.AssignUserToRolesAsync(null, adminUser.Id, new[] { administratorsRole.Id });
                    }
                }

                dataSettings.AdminPassword = null;
                DataSettingsManager.SaveSettings(dataSettings);
            }

            foreach (int tenantId in tenantIds)
            {
                await membershipService.EnsureAdminRoleForTenantAsync(tenantId);
            }
        }

        private static async Task EnsureSettingsAsync(IEnumerable<int> tenantIds)
        {
            var settingsRepository = EngineContext.Current.Resolve<IRepository<Setting>>();
            var allSettings = EngineContext.Current.ResolveAll<ISettings>();
            var allSettingNames = allSettings.Select(x => x.Name).ToList();

            #region NULL Tenant (In case we want default settings)

            var installedSettings = await settingsRepository.FindAsync(x => x.TenantId == null);
            var installedSettingNames = installedSettings.Select(x => x.Name).ToList();

            var settingsToAdd = allSettings.Where(x => x.IsTenantRestricted && !installedSettingNames.Contains(x.Name)).Select(x => new Setting
            {
                Id = Guid.NewGuid(),
                TenantId = null,
                Name = x.Name,
                Type = x.GetType().FullName,
                Value = Activator.CreateInstance(x.GetType()).JsonSerialize()
            }).ToList();

            if (!settingsToAdd.IsNullOrEmpty())
            {
                await settingsRepository.InsertAsync(settingsToAdd);
            }

            var settingsToDelete = installedSettings.Where(x => !allSettingNames.Contains(x.Name)).ToList();

            if (!settingsToDelete.IsNullOrEmpty())
            {
                await settingsRepository.DeleteAsync(settingsToDelete);
            }

            #endregion NULL Tenant (In case we want default settings)

            #region Tenants

            foreach (var tenantId in tenantIds)
            {
                installedSettings = await settingsRepository.FindAsync(x => x.TenantId == tenantId);
                installedSettingNames = installedSettings.Select(x => x.Name).ToList();

                settingsToAdd = allSettings.Where(x => !x.IsTenantRestricted && !installedSettingNames.Contains(x.Name)).Select(x => new Setting
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Name = x.Name,
                    Type = x.GetType().FullName,
                    Value = Activator.CreateInstance(x.GetType()).JsonSerialize()
                }).ToList();

                if (!settingsToAdd.IsNullOrEmpty())
                {
                    await settingsRepository.InsertAsync(settingsToAdd);
                }

                settingsToDelete = installedSettings.Where(x => !allSettingNames.Contains(x.Name)).ToList();

                if (!settingsToDelete.IsNullOrEmpty())
                {
                    await settingsRepository.DeleteAsync(settingsToDelete);
                }
            }

            #endregion Tenants
        }
    }
}