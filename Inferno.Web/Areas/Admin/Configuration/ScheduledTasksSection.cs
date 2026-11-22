using Inferno.Security;
using Inferno.Web.Configuration;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.Areas.Admin.Configuration;

/// <summary>The scheduled-task tile. Installed jobs come from <c>ITask</c>, not from this class.</summary>
public sealed class ScheduledTasksSection(IStringLocalizer localizer) : ISettingsSection
{
    /// <summary>Route of the scheduled-task page. The page's route attribute uses this value.</summary>
    public const string Path = "/admin/configuration/scheduled-tasks";

    /// <inheritdoc />
    public string Title => localizer[InfernoWebLocalizableStrings.ScheduledTasks.Title];

    /// <inheritdoc />
    public string Description => localizer[InfernoWebLocalizableStrings.ScheduledTasks.CardDescription];

    /// <inheritdoc />
    public string Icon => "clock";

    /// <inheritdoc />
    public string Url => Path;

    /// <inheritdoc />
    public string Permission => StandardPolicies.FullAccess;
}
