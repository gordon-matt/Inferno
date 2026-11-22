// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Adds a password to an account that signed in without one.</summary>
public class SetPasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Loads the user and adds the password.</param>
    /// <param name="signInManager">Refreshes the sign-in after the password is added.</param>
    public SetPasswordModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>The password posted by the form.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>A message from a successful save, shown on the next request.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>The password posted when the account does not have one yet.</summary>
    public class InputModel
    {
        /// <summary>The password to add.</summary>
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        /// <summary>The password typed a second time. It must match <see cref="NewPassword"/>.</summary>
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    /// <summary>Shows this form, or the change-password page when the account already has a password.</summary>
    /// <returns>This page, a redirect to change the password, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool hasPassword = await _userManager.HasPasswordAsync(user);

        return hasPassword ? RedirectToPage("./ChangePassword") : Page();
    }

    /// <summary>Adds the posted password and refreshes the sign-in.</summary>
    /// <returns>A redirect back to this page after success, or this page with the Identity errors. Not found when the user is gone.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, Input.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            foreach (var error in addPasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your password has been set.";

        return RedirectToPage();
    }
}