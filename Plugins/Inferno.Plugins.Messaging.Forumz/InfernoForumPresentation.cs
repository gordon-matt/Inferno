using Forumz;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>Each tenant chooses whether the forum keeps its own top bar.</summary>
public sealed class InfernoForumPresentation(ForumSettings settings) : IForumPresentation
{
    /// <inheritdoc />
    public bool UseOwnTopBar => settings.OpenInNewTab;
}
