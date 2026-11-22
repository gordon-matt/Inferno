// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Confirms a new email address and copies it to the user name.</summary>
public class ConfirmEmailChangeModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Changes the email and the user name.</param>
    /// <param name="signInManager">Refreshes the sign-in after both changes succeed.</param>
    public ConfirmEmailChangeModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>The result of the email change, shown on this page.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>Changes the email to <paramref name="email"/> and, when that succeeds, sets the user name to the same address.</summary>
    /// <param name="userId">The user id. <see langword="null"/> redirects to the site root.</param>
    /// <param name="email">The new email address. <see langword="null"/> redirects to the site root.</param>
    /// <param name="code">The Base64Url-encoded change token. <see langword="null"/> redirects to the site root.</param>
    /// <returns>This page after the change is attempted, a redirect when a value is missing, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnGetAsync(string userId, string email, string code)
    {
        if (userId == null || email == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ChangeEmailAsync(user, email, code);
        if (!result.Succeeded)
        {
            StatusMessage = "Error changing email.";
            return Page();
        }

        // In our UI email and user name are one and the same, so when we update the email
        // we need to update the user name.
        var setUserNameResult = await _userManager.SetUserNameAsync(user, email);
        if (!setUserNameResult.Succeeded)
        {
            StatusMessage = "Error changing user name.";
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Thank you for confirming your email change.";
        return Page();
    }
}