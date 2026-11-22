using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.AccountViewModels;

/// <summary>The email address posted to the forgot-password form.</summary>
public class ForgotPasswordViewModel
{
    /// <summary>The account email that should receive a reset link.</summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}