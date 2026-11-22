// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Shows the address that was registered and a confirmation link for that account.</summary>
[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _sender;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Finds the user and creates the confirmation token.</param>
    /// <param name="sender">The email sender. This page does not send mail. It builds a confirmation link instead.</param>
    public RegisterConfirmationModel(UserManager<ApplicationUser> userManager, IEmailSender sender)
    {
        _userManager = userManager;
        _sender = sender;
    }

    /// <summary>The email address that was just registered.</summary>
    public string Email { get; set; }

    /// <summary>Whether the page should show <see cref="EmailConfirmationUrl"/>. This handler always sets it to <see langword="true"/>.</summary>
    public bool DisplayConfirmAccountLink { get; set; }

    /// <summary>A link that confirms the account. Filled whenever the user exists.</summary>
    public string EmailConfirmationUrl { get; set; }

    /// <summary>Loads the confirmation link for <paramref name="email"/>.</summary>
    /// <param name="email">The address that was registered. <see langword="null"/> redirects to the site root.</param>
    /// <param name="returnUrl">Carried into the confirmation link. <see langword="null"/> becomes the site root.</param>
    /// <returns>This page, a redirect when <paramref name="email"/> is missing, or not found when no user has that email.</returns>
    public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
    {
        if (email == null)
        {
            return RedirectToPage("/Index");
        }
        returnUrl ??= Url.Content("~/");

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        Email = email;
        // Once you add a real email sender, you should remove this code that lets you confirm the account
        DisplayConfirmAccountLink = true;
        if (DisplayConfirmAccountLink)
        {
            string userId = await _userManager.GetUserIdAsync(user);
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId, code, returnUrl },
                protocol: Request.Scheme);
        }

        return Page();
    }
}