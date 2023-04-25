using Blazorise;
using Dependo;
using Inferno.Security;
using Inferno.Web.Navigation;
using Inferno.Web.Security;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.Areas.Admin
{
    public class AdminNavigationProvider : INavigationProvider
    {
        public AdminNavigationProvider()
        {
            T = EngineContext.Current.Resolve<IStringLocalizer>();
        }

        public IStringLocalizer T { get; set; }

        #region INavigationProvider Members

        public string MenuName => InfernoWebConstants.Areas.Admin;

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T[InfernoWebLocalizableStrings.General.Home], "0", BuildHomeMenu);
            builder.Add(T[InfernoWebLocalizableStrings.Membership.Title], "1", BuildMembershipMenu);
            builder.Add(T[InfernoWebLocalizableStrings.General.Configuration], "2", BuildConfigurationMenu);
            builder.Add(T[InfernoWebLocalizableStrings.Maintenance.Title], "3", BuildMaintenanceMenu);
            //builder.Add(T[InfernoWebLocalizableStrings.Plugins.Title], "99999", BuildPluginsMenu);
        }

        #endregion INavigationProvider Members

        private static void BuildHomeMenu(NavigationItemBuilder builder)
        {
            builder.Permission(StandardPolicies.AdminAccess);
            builder.Icon(IconName.Home).Url("/admin/index");
        }

        private void BuildMembershipMenu(NavigationItemBuilder builder)
        {
            builder
                .Url("/admin/membership/index")
                .Icon(IconName.Users)
                .Permission(InfernoWebPolicies.MembershipManage);
        }

        private void BuildConfigurationMenu(NavigationItemBuilder builder)
        {
            builder.Icon(IconName.Star);

            // Localization
            builder.Add(T[InfernoWebLocalizableStrings.Localization.Title], "5", item => item
                .Url("/admin/localization/index")
                .Icon(IconName.Language)
                .Permission(InfernoWebPolicies.LanguagesRead));

            //// Indexing
            //builder.Add(T[InfernoWebLocalizableStrings.Indexing.Title], "5", item => item
            //    .Url("/admin/indexing/index")
            //    .Icon(IconName.Search)
            //    .Permission(StandardPolicies.FullAccess));

            // Plugins
            builder.Add(T[InfernoWebLocalizableStrings.Plugins.Title], "5", item => item
                .Url("/admin/plugins/index")
                .Icon(IconName.PuzzlePiece)
                .Permission(StandardPolicies.FullAccess));

            // Settings
            builder.Add(T[InfernoWebLocalizableStrings.General.Settings], "5", item => item
                .Url("/admin/configuration/settings/index")
                .Icon(IconName.Star)
                .Permission(InfernoWebPolicies.SettingsRead));

            // Tenants
            builder.Add(T[InfernoWebLocalizableStrings.Tenants.Title], "5", item => item
                .Url("/admin/tenants/index")
                .Icon(IconName.Building)
                .Permission(StandardPolicies.FullAccess));

            //// Themes
            //builder.Add(T[InfernoWebLocalizableStrings.General.Themes], "5", item => item
            //    .Url("/admin/configuration/themes/index")
            //    .Icon(IconName.Tint)
            //    .Permission(InfernoWebPolicies.ThemesRead));
        }

        private void BuildMaintenanceMenu(NavigationItemBuilder builder)
        {
            builder.Icon(IconName.Wrench);

            // Log
            builder.Add(T[InfernoWebLocalizableStrings.Log.Title], "5", item => item
                .Url("/admin/log/index")
                .Icon(IconName.ExclamationTriangle)
                .Permission(InfernoWebPolicies.LogRead));

            // Scheduled Tasks
            builder.Add(T[InfernoWebLocalizableStrings.ScheduledTasks.Title], "5", item => item
                .Url("/admin/scheduled-tasks/index")
                .Icon(IconName.Clock)
                .Permission(InfernoWebPolicies.ScheduledTasksRead));
        }

        //private void BuildPluginsMenu(NavigationItemBuilder builder)
        //{
        //    builder.Icon("fa fa-puzzle-piece");
        //}
    }
}