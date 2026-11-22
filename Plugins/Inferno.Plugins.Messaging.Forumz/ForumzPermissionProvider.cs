using Inferno.Web.Security.Permissions;

namespace Inferno.Plugins.Messaging.Forumz;

/// <summary>Permission names for the forum. Each value is a policy name and a stored claim value.</summary>
public static class ForumzPolicies
{
    /// <summary>Start topics and reply. Does not open the admin area by itself.</summary>
    public const string Write = "Inferno.Forumz.Write";

    /// <summary>Create forums, move topics, and edit any post. Implies <see cref="Write"/>.</summary>
    public const string Moderate = "Inferno.Forumz.Moderate";
}

/// <summary>The permissions defined by the forum plugin.</summary>
public sealed class ForumzPermissionProvider : IPermissionProvider
{
    /// <inheritdoc />
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
