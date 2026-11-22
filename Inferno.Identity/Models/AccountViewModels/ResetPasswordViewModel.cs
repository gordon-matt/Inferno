using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.AccountViewModels;

/// <summary>The email, new password, and reset token posted to the reset form.</summary>
public class ResetPasswordViewModel
{
    /// <summary>The account email.</summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>The new password.</summary>
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    /// <summary>The new password typed a second time. It must match <see cref="Password"/>.</summary>
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    /// <summary>The password-reset token from the email link.</summary>
    public string Code { get; set; }
}