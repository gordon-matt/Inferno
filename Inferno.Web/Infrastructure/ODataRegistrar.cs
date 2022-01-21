using Extenso.AspNetCore.OData;
using Inferno.Localization.Entities;
using Inferno.Security.Membership;
using Inferno.Tasks.Entities;
using Inferno.Tenants.Entities;
using Inferno.Web.Configuration.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;

namespace Inferno.Web.Infrastructure
{
    public class ODataRegistrar : IODataRegistrar
    {
        #region IODataRegistrar Members

        public void Register(ODataOptions options)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            //// Configuration
            //builder.EntitySet<Setting>("SettingsApi");
            ////builder.EntitySet<EdmThemeConfiguration>("ThemeApi");

            //// Localization
            //builder.EntitySet<Language>("LanguageApi");
            //builder.EntitySet<LocalizableString>("LocalizableStringApi");

            ////// Log
            ////builder.EntitySet<LogEntry>("LogApi");

            //// Membership
            //builder.EntitySet<InfernoPermission>("PermissionApi");
            //builder.EntitySet<InfernoRole>("RoleApi");
            //builder.EntitySet<InfernoUser>("UserApi");
            ////builder.EntitySet<PublicUserInfo>("PublicUserApi");

            ////// Plugins
            ////builder.EntitySet<EdmPluginDescriptor>("PluginApi");

            //// Scheduled Tasks
            //builder.EntitySet<ScheduledTask>("ScheduledTaskApi");

            // Tenants
            builder.EntitySet<Tenant>("TenantApi");

            //RegisterLanguageODataActions(builder);
            //RegisterLocalizableStringODataActions(builder);
            //RegisterLogODataActions(builder);
            //RegisterMembershipODataActions(builder);
            //RegisterPluginODataActions(builder);
            //RegisterScheduledTaskODataActions(builder);
            //RegisterThemeODataActions(builder);

            options.AddRouteComponents("odata/inferno/web", builder.GetEdmModel());
        }

        #endregion IODataRegistrar Members

        //private static void RegisterLogODataActions(ODataModelBuilder builder)
        //{
        //    var clearAction = builder.EntityType<LogEntry>().Collection.Action("Clear");
        //    clearAction.Returns<IActionResult>();
        //}

        //private static void RegisterMembershipODataActions(ODataModelBuilder builder)
        //{
        //    var getUsersInRoleFunction = builder.EntityType<InfernoUser>().Collection.Function("GetUsersInRole");
        //    getUsersInRoleFunction.Parameter<string>("roleId");
        //    getUsersInRoleFunction.Returns<IActionResult>();

        //    var assignUserToRolesAction = builder.EntityType<InfernoUser>().Collection.Action("AssignUserToRoles");
        //    assignUserToRolesAction.Parameter<string>("userId");
        //    assignUserToRolesAction.CollectionParameter<string>("roles");
        //    assignUserToRolesAction.Returns<IActionResult>();

        //    var changePasswordAction = builder.EntityType<InfernoUser>().Collection.Action("ChangePassword");
        //    changePasswordAction.Parameter<string>("userId");
        //    changePasswordAction.Parameter<string>("password");
        //    changePasswordAction.Returns<IActionResult>();

        //    var getRolesForUserFunction = builder.EntityType<InfernoRole>().Collection.Function("GetRolesForUser");
        //    getRolesForUserFunction.Parameter<string>("userId");
        //    getRolesForUserFunction.Returns<IActionResult>();

        //    var assignPermissionsToRoleAction = builder.EntityType<InfernoRole>().Collection.Action("AssignPermissionsToRole");
        //    assignPermissionsToRoleAction.Parameter<string>("roleId");
        //    assignPermissionsToRoleAction.CollectionParameter<string>("permissions");
        //    assignPermissionsToRoleAction.Returns<IActionResult>();

        //    var getPermissionsForRoleFunction = builder.EntityType<InfernoPermission>().Collection.Function("GetPermissionsForRole");
        //    getPermissionsForRoleFunction.Parameter<string>("roleId");
        //    getPermissionsForRoleFunction.Returns<IActionResult>();
        //}

        //private static void RegisterPluginODataActions(ODataModelBuilder builder)
        //{
        //    var installAction = builder.EntityType<EdmPluginDescriptor>().Collection.Action("Install");
        //    installAction.Parameter<string>("systemName");
        //    installAction.Returns<IHttpActionResult>();

        //    var uninstallAction = builder.EntityType<EdmPluginDescriptor>().Collection.Action("Uninstall");
        //    uninstallAction.Parameter<string>("systemName");
        //    uninstallAction.Returns<IHttpActionResult>();
        //}

        //private static void RegisterScheduledTaskODataActions(ODataModelBuilder builder)
        //{
        //    var runNowAction = builder.EntityType<ScheduledTask>().Collection.Action("RunNow");
        //    runNowAction.Parameter<int>("taskId");
        //    runNowAction.Returns<IActionResult>();
        //}

        //private static void RegisterThemeODataActions(ODataModelBuilder builder)
        //{
        //    var setDesktopThemeAction = builder.EntityType<EdmThemeConfiguration>().Collection.Action("SetTheme");
        //    setDesktopThemeAction.Parameter<string>("themeName");
        //    setDesktopThemeAction.Returns<IActionResult>();
        //}

        //private static void RegisterLanguageODataActions(ODataModelBuilder builder)
        //{
        //    var resetLocalizableStringsAction = builder.EntityType<Language>().Collection.Action("ResetLocalizableStrings");
        //    resetLocalizableStringsAction.Returns<IActionResult>();
        //}

        //private static void RegisterLocalizableStringODataActions(ODataModelBuilder builder)
        //{
        //    var getComparitiveTableFunction = builder.EntityType<LocalizableString>().Collection.Function("GetComparitiveTable");
        //    getComparitiveTableFunction.Parameter<string>("cultureCode");
        //    getComparitiveTableFunction.Returns<IActionResult>();

        //    var putComparitiveAction = builder.EntityType<LocalizableString>().Collection.Action("PutComparitive");
        //    putComparitiveAction.Parameter<string>("cultureCode");
        //    putComparitiveAction.Parameter<string>("key");
        //    putComparitiveAction.Parameter<ComparitiveLocalizableString>("entity");

        //    var deleteComparitiveAction = builder.EntityType<LocalizableString>().Collection.Action("DeleteComparitive");
        //    deleteComparitiveAction.Parameter<string>("cultureCode");
        //    deleteComparitiveAction.Parameter<string>("key");
        //}
    }
}