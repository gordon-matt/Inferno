using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.ManageViewModels;

/// <summary>The shared key and verification code for turning on an authenticator app.</summary>
public class EnableAuthenticatorViewModel
{
    /// <summary>The verification code from the authenticator app. Spaces and hyphens are removed before it is checked.</summary>
    [Required]
    [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Text)]
    [Display(Name = "Verification Code")]
    public string Code { get; set; }

    /// <summary>The authenticator key, grouped and shown in lower case.</summary>
    [ReadOnly(true)]
    public string SharedKey { get; set; }

    /// <summary>The otpauth URI for the QR code.</summary>
    public string AuthenticatorUri { get; set; }
}