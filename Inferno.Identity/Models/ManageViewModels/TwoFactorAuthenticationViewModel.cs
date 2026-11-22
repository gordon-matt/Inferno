namespace Inferno.Identity.Models.ManageViewModels;

/// <summary>The two-factor status shown on the management page.</summary>
public class TwoFactorAuthenticationViewModel
{
    /// <summary>Whether an authenticator key has been stored.</summary>
    public bool HasAuthenticator { get; set; }

    /// <summary>How many recovery codes remain.</summary>
    public int RecoveryCodesLeft { get; set; }

    /// <summary>Whether two-factor authentication is turned on.</summary>
    public bool Is2faEnabled { get; set; }
}