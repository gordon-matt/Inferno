using Forumz;
using Forumz.Data.Sql;
using Forumz.Services;
using Inferno.Plugins.Messaging.Forumz;
using Inferno.Web.ContentManagement.Infrastructure;
using Inferno.Web.Infrastructure;
using Inferno.Web.Navigation;
using Inferno.Web.Security.Permissions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Pluginz;
using Pluginz.Web;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>
/// Mounts the Forumz Blazor circuit at /forums. The library owns the pages; this plugin supplies
/// Inferno's tenant, permissions and database.
/// </summary>
public sealed class ForumzPlugin : PluginBase, IWebPlugin
{
    public override void ConfigureServices(IServiceCollection services, PluginContext context)
    {
        var installed = DataSettingsManager.LoadSettings();

        services.AddForumz(options =>
        {
            options.Title = "Forums";
            options.HomePath = "/";
            options.LogoutPath = "/Identity/Account/LogOut";
        });

        if (!string.IsNullOrWhiteSpace(installed.ConnectionString))
        {
            services.AddForumzSqlServer(installed.ConnectionString);
        }

        // Registered after AddForumz so these replace the single-tenant defaults.
        services.AddScoped<IForumTenantScope, InfernoForumTenantScope>();
        services.AddScoped<IForumAccess, InfernoForumAccess>();
        services.AddSingleton<IPermissionProvider, ForumzPermissionProvider>();
        services.AddSingleton<INavigationProvider, ForumzNavigationProvider>();
        services.AddSingleton<IAutoMenuProvider, ForumzAutoMenuProvider>();
    }

    /// <summary>The forum is its own Blazor document, mounted beside Inferno's router.</summary>
    public void MapEndpoints(IEndpointRouteBuilder endpoints, PluginRuntimeContext context) =>
        endpoints.MapForumz();
}
