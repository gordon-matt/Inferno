namespace Inferno.Web.Security.Permissions;

/// <summary>
/// A right that can be granted to a role or to a user. <see cref="Name"/> is the authorization policy name,
/// so <c>[Authorize(Policy = ...)]</c> works with it directly, and it is the value of the stored claim.
/// </summary>
public sealed record Permission(string Name, string Category, string DisplayName)
{
    /// <summary>The claim type grants are stored under, in the Identity role and user claim tables.</summary>
    public const string ClaimType = "permission";

    public string Description { get; init; }

    /// <summary>Lesser permissions that come with this one, so that granting Write is enough to read as well.</summary>
    public IReadOnlyList<string> Implies { get; init; } = [];

    /// <summary>
    /// Whether holding this permission lets the user into the admin area. Turn it off for a front-end right,
    /// such as posting to a forum, that should not open the back office.
    /// </summary>
    public bool GrantsAdminAccess { get; init; } = true;
}
