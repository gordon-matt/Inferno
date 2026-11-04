namespace Inferno.Security;

public static class StandardPolicies
{
    /// <summary>Opens the admin area. Every admin permission brings it along, so it rarely needs granting on its own.</summary>
    public const string AdminAccess = "Inferno.AdminAccess";

    /// <summary>Every permission, including ones added later. Held by the host's Administrators role.</summary>
    public const string FullAccess = "Inferno.FullAccess";
}
