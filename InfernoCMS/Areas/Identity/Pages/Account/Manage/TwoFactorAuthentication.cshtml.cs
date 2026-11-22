// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Shows two-factor status and forgets this browser's remembered authenticator.</summary>
public class TwoFactorAuthenticationModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<TwoFactorAuthenticationModel> _logger;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Loads the authenticator, two-factor flag, and recovery-code count.</param>
    /// <param name="signInManager">Reports whether this browser is remembered and forgets it.</param>
    /// <param name="logger">The page logger. This page does not write to it.</param>
    public TwoFactorAuthenticationModel(
        UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ILogger<TwoFactorAuthenticationModel> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>Whether an authenticator key has been stored.</summary>
    public bool HasAuthenticator { get; set; }

    /// <summary>How many recovery codes remain.</summary>
    public int RecoveryCodesLeft { get; set; }

    /// <summary>Whether two-factor authentication is turned on.</summary>
    [BindProperty]
    public bool Is2faEnabled { get; set; }

    /// <summary>Whether this browser is remembered for two-factor sign-in.</summary>
    public bool IsMachineRemembered { get; set; }

    /// <summary>A message from forgetting this browser, shown on the next request.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>Loads the authenticator, two-factor flag, remembered-browser flag, and recovery-code count.</summary>
    /// <returns>This page, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null;
        Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user);
        RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user);

        return Page();
    }

    /// <summary>Forgets this browser, so the next sign-in from it asks for an authenticator code.</summary>
    /// <returns>A redirect back to this page, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await _signInManager.ForgetTwoFactorClientAsync();
        StatusMessage = "The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.";
        return RedirectToPage();
    }
}