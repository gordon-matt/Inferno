using Dependo;
using Inferno.Security;
using Inferno.Web.Navigation;
using Inferno.Web.Security;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.Areas.Admin;

public class AdminNavigationProvider : INavigationProvider
{
    public AdminNavigationProvider()
    {
        T = DependoResolver.Instance.Resolve<IStringLocalizer>();
    }

    public IStringLocalizer T { get; set; }

    #region INavigationProvider Members

    public string MenuName => InfernoWebConstants.Areas.Admin;

    public void GetNavigation(NavigationBuilder builder)
    {
        builder.Add(T[InfernoWebLocalizableStrings.Dashboard.Title], "0", BuildDashboardMenu);
        builder.Add(T[InfernoWebLocalizableStrings.Membership.Title], "1", BuildMembershipMenu);
        builder.Add(T[InfernoWebLocalizableStrings.General.Configuration], "2", BuildConfigurationMenu);
        builder.Add(T[InfernoWebLocalizableStrings.Maintenance.Title], "3", BuildMaintenanceMenu);
    }

    #endregion INavigationProvider Members

    private static void BuildDashboardMenu(NavigationItemBuilder builder)
    {
        builder.Permission(StandardPolicies.AdminAccess);
        builder.Icon("fas fa-tachometer-alt").Url("/admin");
    }

    private void BuildMembershipMenu(NavigationItemBuilder builder) => builder
            .Url("/admin/membership/users/index")
            .Icon("fas fa-users")
            .Permission(InfernoWebPolicies.MembershipManage);

    private void BuildConfigurationMenu(NavigationItemBuilder builder)
    {
        builder.Icon("fas fa-star");

        // Localization
        builder.Add(T[InfernoWebLocalizableStrings.Localization.Title], "5", item => item
            .Url("/admin/localization/index")
            .Icon("fas fa-language")
            .Permission(InfernoWebPolicies.LanguagesRead));

        //// Indexing — TODO: not yet ported from MantleCMS
        //builder.Add(T[InfernoWebLocalizableStrings.Indexing.Title], "5", item => item
        //    .Url("/admin/indexing/index")
        //    .Icon("fas fa-search")
        //    .Permission(StandardPolicies.FullAccess));

        // Plugins
        builder.Add(T[InfernoWebLocalizableStrings.Plugins.Title], "5", item => item
            .Url("/admin/plugins")
            .Icon("fas fa-puzzle-piece")
            .Permission(InfernoWebPolicies.PluginsRead));

        // Settings
        builder.Add(T[InfernoWebLocalizableStrings.General.Settings], "5", item => item
            .Url("/admin/configuration/settings/index")
            .Icon("fas fa-star")
            .Permission(InfernoWebPolicies.SettingsRead));

        // Tenants
        builder.Add(T[InfernoWebLocalizableStrings.Tenants.Title], "5", item => item
            .Url("/admin/tenants/index")
            .Icon("fas fa-building")
            .Permission(StandardPolicies.FullAccess));

        // Themes
        builder.Add(T[InfernoWebLocalizableStrings.General.Themes], "5", item => item
            .Url("/admin/configuration/themes/index")
            .Icon("fas fa-tint")
            .Permission(InfernoWebPolicies.ThemesRead));
    }

    private void BuildMaintenanceMenu(NavigationItemBuilder builder)
    {
        builder.Icon("fas fa-wrench");

        // Log
        builder.Add(T[InfernoWebLocalizableStrings.Log.Title], "5", item => item
            .Url("/admin/log/index")
            .Icon("fas fa-exclamation-triangle")
            .Permission(InfernoWebPolicies.LogRead));

        // Scheduled Tasks
        builder.Add(T[InfernoWebLocalizableStrings.ScheduledTasks.Title], "5", item => item
            .Url("/admin/scheduled-tasks/index")
            .Icon("fas fa-clock")
            .Permission(InfernoWebPolicies.ScheduledTasksRead));
    }

    //private void BuildPluginsMenu(NavigationItemBuilder builder)
    //{
    //    builder.Icon("fa fa-puzzle-piece");
    //}
}