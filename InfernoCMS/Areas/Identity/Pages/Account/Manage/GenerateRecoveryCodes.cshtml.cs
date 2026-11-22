// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Replaces the recovery codes with ten new ones when two-factor authentication is on.</summary>
public class GenerateRecoveryCodesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GenerateRecoveryCodesModel> _logger;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Checks two-factor authentication and generates the codes.</param>
    /// <param name="logger">Records the generation.</param>
    public GenerateRecoveryCodesModel(
        UserManager<ApplicationUser> userManager,
        ILogger<GenerateRecoveryCodesModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>The ten new recovery codes, carried to the show page.</summary>
    [TempData]
    public string[] RecoveryCodes { get; set; }

    /// <summary>A message that new codes were generated, shown with the codes.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>Shows the warning before the codes are replaced.</summary>
    /// <returns>This page, or not found when the user is gone.</returns>
    /// <exception cref="InvalidOperationException">Two-factor authentication is off.</exception>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        return !isTwoFactorEnabled
            ? throw new InvalidOperationException($"Cannot generate recovery codes for user because they do not have 2FA enabled.")
            : (IActionResult)Page();
    }

    /// <summary>Generates ten new recovery codes and discards the previous ones.</summary>
    /// <returns>A redirect to the page that shows the codes, or not found when the user is gone.</returns>
    /// <exception cref="InvalidOperationException">Two-factor authentication is off.</exception>
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        bool isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        string userId = await _userManager.GetUserIdAsync(user);
        if (!isTwoFactorEnabled)
        {
            throw new InvalidOperationException($"Cannot generate recovery codes for user as they do not have 2FA enabled.");
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        RecoveryCodes = recoveryCodes.ToArray();

        _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);
        StatusMessage = "You have generated new recovery codes.";
        return RedirectToPage("./ShowRecoveryCodes");
    }
}