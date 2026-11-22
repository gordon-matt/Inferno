// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Deletes the signed-in user after checking the password when the account has one.</summary>
public class DeletePersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<DeletePersonalDataModel> _logger;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Checks the password and deletes the user.</param>
    /// <param name="signInManager">Signs the user out after the delete succeeds.</param>
    /// <param name="logger">Records the deletion.</param>
    public DeletePersonalDataModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<DeletePersonalDataModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>The password posted when the account has one.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>The password required to delete an account that has one.</summary>
    public class InputModel
    {
        /// <summary>The current password.</summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    /// <summary>Whether the form must include <see cref="InputModel.Password"/>.</summary>
    public bool RequirePassword { get; set; }

    /// <summary>Shows the delete form and whether a password is required.</summary>
    /// <returns>This page, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        return Page();
    }

    /// <summary>Deletes the user after a password check when one is required, then signs out.</summary>
    /// <returns>A redirect to the site root, or this page when the password is wrong. Not found when the user is gone.</returns>
    /// <exception cref="InvalidOperationException">Identity rejects the delete.</exception>
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        RequirePassword = await _userManager.HasPasswordAsync(user);
        if (RequirePassword)
        {
            if (!await _userManager.CheckPasswordAsync(user, Input.Password))
            {
                ModelState.AddModelError(string.Empty, "Incorrect password.");
                return Page();
            }
        }

        var result = await _userManager.DeleteAsync(user);
        string userId = await _userManager.GetUserIdAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Unexpected error occurred deleting user.");
        }

        await _signInManager.SignOutAsync();

        _logger.LogInformation("User with ID '{UserId}' deleted themselves.", userId);

        return Redirect("~/");
    }
}