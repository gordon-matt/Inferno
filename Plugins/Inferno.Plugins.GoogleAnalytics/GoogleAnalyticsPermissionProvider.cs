using Inferno.Web.Security.Permissions;

namespace Inferno.Plugins.GoogleAnalytics;

/// <summary>Permission names for Google Analytics.</summary>
public static class GoogleAnalyticsPolicies
{
    /// <summary>Save the measurement id, the Analytics property, and the service account used by the dashboard.</summary>
    public const string Manage = "Inferno.GoogleAnalytics.Manage";
}

/// <summary>The permission defined by the Google Analytics plugin.</summary>
public sealed class GoogleAnalyticsPermissionProvider : IPermissionProvider
{
    /// <inheritdoc />
    public IEnumerable<Permission> GetPermissions() =>
    [
        new(GoogleAnalyticsPolicies.Manage, "Google Analytics", "Manage Google Analytics")
        {
            Description = "Save the measurement ID, the Analytics property and the service account used by the dashboard.",
        },
    ];
}
