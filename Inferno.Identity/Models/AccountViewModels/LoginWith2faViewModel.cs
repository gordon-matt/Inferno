using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.AccountViewModels;

/// <summary>The authenticator step that follows a successful password check.</summary>
public class LoginWith2faViewModel
{
    /// <summary>The code from the authenticator app. Spaces and hyphens are removed before it is checked.</summary>
    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Authenticator code")]
    public string TwoFactorCode { get; set; }

    /// <summary>Whether this browser should skip authenticator prompts later.</summary>
    [Display(Name = "Remember this machine")]
    public bool RememberMachine { get; set; }

    /// <summary>Whether the sign-in cookie should persist. Copied from the password step.</summary>
    public bool RememberMe { get; set; }
}