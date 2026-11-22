// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Sets a new password from the token in the reset link.</summary>
public class ResetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>Creates the page with <paramref name="userManager"/>.</summary>
    /// <param name="userManager">Finds the user and resets the password.</param>
    public ResetPasswordModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>The email, new password, and reset token posted by the form.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>The fields posted by the reset form.</summary>
    public class InputModel
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

        /// <summary>The password-reset token, decoded from the link.</summary>
        [Required]
        public string Code { get; set; }
    }

    /// <summary>Decodes <paramref name="code"/> onto the form.</summary>
    /// <param name="code">The Base64Url-encoded reset token from the email link.</param>
    /// <returns>This page, or a bad request when <paramref name="code"/> is <see langword="null"/>.</returns>
    public IActionResult OnGet(string code = null)
    {
        if (code == null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }
        else
        {
            Input = new InputModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
            };
            return Page();
        }
    }

    /// <summary>Resets the password. A missing account is sent to the confirmation page.</summary>
    /// <returns>The confirmation page after success or a missing account. Otherwise this page with the Identity errors.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
        if (result.Succeeded)
        {
            return RedirectToPage("./ResetPasswordConfirmation");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
        return Page();
    }
}