using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.AccountViewModels;

/// <summary>The email and password posted to the login form.</summary>
public class LoginViewModel
{
    /// <summary>The account email used to sign in.</summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>The account password.</summary>
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    /// <summary>Whether the sign-in cookie should persist after the browser closes.</summary>
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}