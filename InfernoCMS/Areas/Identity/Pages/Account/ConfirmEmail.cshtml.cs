// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Confirms an email address from the link in the confirmation message.</summary>
public class ConfirmEmailModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>Creates the page with <paramref name="userManager"/>.</summary>
    /// <param name="userManager">Finds the user and confirms the email.</param>
    public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>The confirmation result, shown on this page.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>Confirms the email for <paramref name="userId"/> with the decoded <paramref name="code"/>.</summary>
    /// <param name="userId">The user id. <see langword="null"/> redirects to the site root.</param>
    /// <param name="code">The Base64Url-encoded confirmation token. <see langword="null"/> redirects to the site root.</param>
    /// <returns>This page after the token is checked, a redirect when either value is missing, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnGetAsync(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToPage("/Index");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
        return Page();
    }
}