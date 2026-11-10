using Inferno.Web.Security.Permissions;

namespace Inferno.Plugins.GoogleAnalytics;

public static class GoogleAnalyticsPolicies
{
    public const string Manage = "Inferno.GoogleAnalytics.Manage";
}

public sealed class GoogleAnalyticsPermissionProvider : IPermissionProvider
{
    public IEnumerable<Permission> GetPermissions() =>
    [
        new(GoogleAnalyticsPolicies.Manage, "Google Analytics", "Manage Google Analytics")
        {
            Description = "Save the measurement ID, the Analytics property and the service account used by the dashboard.",
        },
    ];
}
