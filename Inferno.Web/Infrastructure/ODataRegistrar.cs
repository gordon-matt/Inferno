using Extenso.AspNetCore.OData;
using Inferno.Security.Membership;
using Inferno.Tasks.Entities;
using Inferno.Tenants.Entities;
using Inferno.Web.Areas.Admin.Configuration.Themes.Models;
using Inferno.Web.Configuration.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;

namespace Inferno.Web.Infrastructure;

public class ODataRegistrar : IODataRegistrar
{
    #region IODataRegistrar Members

    public void Register(ODataOptions options)
    {
        ODataModelBuilder builder = new ODataConventionModelBuilder();

        // Configuration
        builder.EntitySet<Setting>("SettingsApi");
        builder.EntitySet<EdmThemeConfiguration>("ThemeApi");

        //// Membership
        //builder.EntitySet<InfernoPermission>("PermissionApi");
        builder.EntitySet<InfernoRole>("RoleApi");
        builder.EntitySet<InfernoUser>("UserApi");
        ////builder.EntitySet<PublicUserInfo>("PublicUserApi");

        // Scheduled Tasks
        builder.EntitySet<ScheduledTask>(InfernoWebConstants.ODataRoutes.EntitySetNames.ScheduledTask);

        // Tenants
        builder.EntitySet<Tenant>(InfernoWebConstants.ODataRoutes.EntitySetNames.Tenant);

        RegisterMembershipODataActions(builder);
        RegisterScheduledTaskODataActions(builder);
        RegisterThemeODataActions(builder);

        options.AddRouteComponents($"odata/{InfernoWebConstants.ODataRoutes.Prefix}", builder.GetEdmModel());
    }

    #endregion IODataRegistrar Members

    private static void RegisterMembershipODataActions(ODataModelBuilder builder)
    {
        var getUsersInRoleFunction = builder.EntityType<InfernoUser>().Collection.Function("GetUsersInRole");
        getUsersInRoleFunction.Parameter<string>("roleId");
        getUsersInRoleFunction.Returns<IActionResult>();

        var assignUserToRolesAction = builder.EntityType<InfernoUser>().Collection.Action("AssignUserToRoles");
        assignUserToRolesAction.Parameter<string>("userId");
        assignUserToRolesAction.CollectionParameter<string>("roles");
        assignUserToRolesAction.Returns<IActionResult>();

        var changePasswordAction = builder.EntityType<InfernoUser>().Collection.Action("ChangePassword");
        changePasswordAction.Parameter<string>("userId");
        changePasswordAction.Parameter<string>("password");
        changePasswordAction.Returns<IActionResult>();

        var getRolesForUserFunction = builder.EntityType<InfernoRole>().Collection.Function("GetRolesForUser");
        getRolesForUserFunction.Parameter<string>("userId");
        getRolesForUserFunction.Returns<IActionResult>();

        //var assignPermissionsToRoleAction = builder.EntityType<InfernoRole>().Collection.Action("AssignPermissionsToRole");
        //assignPermissionsToRoleAction.Parameter<string>("roleId");
        //assignPermissionsToRoleAction.CollectionParameter<string>("permissions");
        //assignPermissionsToRoleAction.Returns<IActionResult>();

        //var getPermissionsForRoleFunction = builder.EntityType<InfernoPermission>().Collection.Function("GetPermissionsForRole");
        //getPermissionsForRoleFunction.Parameter<string>("roleId");
        //getPermissionsForRoleFunction.Returns<IActionResult>();
    }

    private static void RegisterScheduledTaskODataActions(ODataModelBuilder builder)
    {
        var runNowAction = builder.EntityType<ScheduledTask>().Collection.Action("RunNow");
        runNowAction.Parameter<int>("taskId");
        runNowAction.Returns<IActionResult>();
    }

    private static void RegisterThemeODataActions(ODataModelBuilder builder)
    {
        var setThemeAction = builder.EntityType<EdmThemeConfiguration>().Collection.Action("SetTheme");
        setThemeAction.Parameter<string>("themeName");
        setThemeAction.Returns<IActionResult>();
    }
}