using Inferno.Web.Security.Permissions;

namespace Inferno.Plugins.Messaging.Forumz;

public static class ForumzPolicies
{
    public const string Write = "Inferno.Forumz.Write";

    public const string Moderate = "Inferno.Forumz.Moderate";
}

public sealed class ForumzPermissionProvider : IPermissionProvider
{
    public IEnumerable<Permission> GetPermissions() =>
    [
        new(ForumzPolicies.Moderate, "Forums", "Moderate forums")
        {
            Description = "Create forums, move topics, and edit any post.",
            Implies = [ForumzPolicies.Write],
        },
        new(ForumzPolicies.Write, "Forums", "Post in forums")
        {
            Description = "Start topics and reply. Does not open the admin area by itself.",
            GrantsAdminAccess = false,
        },
    ];
}
