namespace Inferno.Security;

/// <summary>Role names the rest of the site looks up by this constant.</summary>
public static class InfernoSecurityConstants
{
    /// <summary>Built-in role names.</summary>
    public static class Roles
    {
        /// <summary>The role that holds every permission. A null tenant id on this role is the host role.</summary>
        public const string Administrators = "Administrators";
    }
}
