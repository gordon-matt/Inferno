using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.AccountViewModels;

/// <summary>A two-factor recovery code posted instead of an authenticator code.</summary>
public class LoginWithRecoveryCodeViewModel
{
    /// <summary>The recovery code. Spaces are removed before it is checked.</summary>
    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Recovery Code")]
    public string RecoveryCode { get; set; }
}