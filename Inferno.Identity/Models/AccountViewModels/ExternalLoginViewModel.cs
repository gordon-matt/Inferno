using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.AccountViewModels;

/// <summary>The email address collected when an external login has no local account yet.</summary>
public class ExternalLoginViewModel
{
    /// <summary>The email address for the new account. It is also stored as the user name.</summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}