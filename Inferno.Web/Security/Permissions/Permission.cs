namespace Inferno.Web.Security.Permissions;

/// <summary>
/// A right that can be granted to a role or to a user. <see cref="Name"/> is the authorization policy name,
/// so <c>[Authorize(Policy = ...)]</c> works with it directly, and it is the value of the stored claim.
/// </summary>
/// <param name="Name">The policy name and the stored claim value.</param>
/// <param name="Category">The group shown in the permission list.</param>
/// <param name="DisplayName">The label shown for this permission.</param>
public sealed record Permission(string Name, string Category, string DisplayName)
{
    /// <summary>The claim type grants are stored under, in the Identity role and user claim tables.</summary>
    public const string ClaimType = "permission";

    /// <summary>Extra text shown with the permission. Empty when the display name is enough.</summary>
    public string Description { get; init; }

    /// <summary>Lesser permissions that come with this one, so that granting Write is enough to read as well.</summary>
    public IReadOnlyList<string> Implies { get; init; } = [];

    /// <summary>
    /// Whether holding this permission lets the user into the admin area. Turn it off for a front-end right,
    /// such as posting to a forum, that should not open the back office.
    /// </summary>
    public bool GrantsAdminAccess { get; init; } = true;
}
