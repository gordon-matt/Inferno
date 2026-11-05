using Dependo;
using Inferno.Tenants.Services;
using Inferno.Web.ContentManagement.Infrastructure;
using Inferno.Web.Infrastructure;
using Kameleon.Resolution;
using Kameleon.Settings;
using Microsoft.Extensions.Hosting;

namespace InfernoCMS.Infrastructure;

/// <summary>
/// Gives each demo tenant its theme. The demo data cannot do it itself: it runs before the application starts,
/// and Kameleon creates its tables when the application starts. This service is registered after Kameleon's
/// own startup, so the tables exist by the time it runs, and it does nothing unless the demo data asked for it.
/// </summary>
/// <remarks>
/// Besides choosing each tenant's theme, it limits the extra themes to the tenants named for them, so the
/// theme catalog shows how a host decides who may use what.
/// </remarks>
public sealed class DemoThemeSeeder(IServiceScopeFactory scopes, ILogger<DemoThemeSeeder> logger) : IHostedService
{
    private const string DEFAULT_THEME = "default";

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var dataSettings = DependoResolver.Instance.Resolve<DataSettings>();
        if (!dataSettings.DemoThemesPending)
        {
            return;
        }

        using var scope = scopes.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IThemeSettingsStore>();
        var tenants = scope.ServiceProvider.GetRequiredService<ITenantService>();
        var notifier = scope.ServiceProvider.GetRequiredService<IThemeChangeNotifier>();

        var themed = DemoTenants.All.Where(demo => demo.ThemeId != DEFAULT_THEME).Select(demo => demo.ThemeId).Distinct();
        foreach (string themeId in themed)
        {
            await store.SaveCatalogAsync(new ThemeCatalogRecord(themeId, IsEnabled: true, Kameleon.ThemeAvailability.SelectedTenants), "demo data", cancellationToken);
        }

        foreach (var demo in DemoTenants.All)
        {
            int? tenantId = FindTenantId(tenants, demo);
            if (tenantId is null)
            {
                logger.LogWarning("The demo tenant {Tenant} was not found, so it keeps the default theme.", demo.Name);
                continue;
            }

            string tenant = tenantId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var configuration = string.IsNullOrEmpty(demo.ThemeAccent)
                ? new Dictionary<string, string>()
                : new Dictionary<string, string> { ["accent"] = demo.ThemeAccent };

            await store.SaveTenantThemeAsync(new TenantThemeRecord(tenant, demo.ThemeId, IsGranted: true, IsEnabled: true, configuration), "demo data", cancellationToken);
            await store.SaveTenantSettingsAsync(new TenantThemeSettingsRecord(tenant, demo.ThemeId, DefaultLayoutId: null, AllowUserOverride: false), "demo data", cancellationToken);
        }

        notifier.AllChanged();

        dataSettings.DemoThemesPending = false;
        DataSettingsManager.SaveSettings(dataSettings);
        logger.LogInformation("Demo themes applied.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static int? FindTenantId(ITenantService tenants, DemoTenant demo)
    {
        using var connection = tenants.OpenConnection();
        var found = connection.Query(tenant => tenant.Name == demo.Name).Select(tenant => (int?)tenant.Id).FirstOrDefault();

        if (found is null && demo.Name == DemoTenants.DefaultTenantName)
        {
            found = connection.Query().OrderBy(tenant => tenant.Id).Select(tenant => (int?)tenant.Id).FirstOrDefault();
        }

        return found;
    }
}
