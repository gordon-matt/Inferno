using Inferno.Web.Configuration;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>Per-tenant choice between the site layout and the forum's own top bar.</summary>
public sealed class ForumSettings : ISettings
{
    /// <summary>
    /// Opens the forum in a new tab with its own top bar. Off draws it under the site menu.
    /// </summary>
    public bool OpenInNewTab { get; set; }

    /// <inheritdoc />
    public string Name => "Forums";

    /// <inheritdoc />
    public bool IsTenantRestricted => false;

    /// <inheritdoc />
    public Type EditorType => typeof(ForumSettingsEditor);
}
