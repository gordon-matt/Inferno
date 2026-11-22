using System.ComponentModel.DataAnnotations;

namespace Inferno.Identity.Models.ManageViewModels;

/// <summary>The profile fields shown on the account management page.</summary>
public class IndexViewModel
{
    /// <summary>The account's sign-in name.</summary>
    public string Username { get; set; }

    /// <summary>Whether the email address has been confirmed.</summary>
    public bool IsEmailConfirmed { get; set; }

    /// <summary>The account email. A changed value is saved.</summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    /// <summary>The account phone number. A changed value is saved.</summary>
    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; }

    /// <summary>A message from the previous manage action.</summary>
    public string StatusMessage { get; set; }
}