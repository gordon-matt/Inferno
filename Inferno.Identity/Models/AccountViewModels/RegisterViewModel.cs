using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.AccountViewModels;

/// <summary>The email and password posted to the registration form.</summary>
public class RegisterViewModel
{
    /// <summary>The email address, which is also stored as the user name.</summary>
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    /// <summary>The password for the new account.</summary>
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; }

    /// <summary>The password typed a second time. It must match <see cref="Password"/>.</summary>
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }
}