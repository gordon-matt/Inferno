using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.ManageViewModels;

/// <summary>The current and new passwords posted to the change-password form.</summary>
public class ChangePasswordViewModel
{
    /// <summary>The current password.</summary>
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string OldPassword { get; set; }

    /// <summary>The replacement password.</summary>
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string NewPassword { get; set; }

    /// <summary>The replacement password typed a second time. It must match <see cref="NewPassword"/>.</summary>
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; }

    /// <summary>A message from the previous manage action.</summary>
    public string StatusMessage { get; set; }
}