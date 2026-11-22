// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Changes the password of an account that already has one.</summary>
public class ChangePasswordModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ChangePasswordModel> _logger;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Loads the user and changes the password.</param>
    /// <param name="signInManager">Refreshes the sign-in after a successful change.</param>
    /// <param name="logger">Records a successful change.</param>
    public ChangePasswordModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<ChangePasswordModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>The current and new passwords posted by the form.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>A message from a successful change, shown on the next request.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>The passwords posted by the change form.</summary>
    public class InputModel
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
    }

    /// <summary>Shows this form, or the set-password page when the account has no password.</summary>
    /// <returns>This page, a redirect to set a password, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool hasPassword = await _userManager.HasPasswordAsync(user);
        return !hasPassword ? RedirectToPage("./SetPassword") : Page();
    }

    /// <summary>Changes the password and refreshes the sign-in.</summary>
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

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        _logger.LogInformation("User changed their password successfully.");
        StatusMessage = "Your password has been changed.";

        return RedirectToPage();
    }
}