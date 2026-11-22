// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Completes sign-in with a recovery code after the password has been accepted.</summary>
public class LoginWithRecoveryCodeModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginWithRecoveryCodeModel> _logger;

    /// <summary>Creates the page with the sign-in services.</summary>
    /// <param name="signInManager">Loads the two-factor user and checks the recovery code.</param>
    /// <param name="userManager">The user manager. This page does not call it.</param>
    /// <param name="logger">Records success, lockout, and a rejected code.</param>
    public LoginWithRecoveryCodeModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginWithRecoveryCodeModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>The recovery code posted by the form.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>The URL to open after sign-in. May be <see langword="null"/> until the form is posted.</summary>
    public string ReturnUrl { get; set; }

    /// <summary>The field posted by the recovery-code form.</summary>
    public class InputModel
    {
        /// <summary>The recovery code. Spaces are removed before it is checked.</summary>
        [BindProperty]
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Recovery Code")]
        public string RecoveryCode { get; set; }
    }

    /// <summary>Shows the recovery-code form for the user who passed the password check.</summary>
    /// <param name="returnUrl">The URL to open after sign-in. May be <see langword="null"/>.</param>
    /// <returns>This page.</returns>
    /// <exception cref="InvalidOperationException">No two-factor user is in progress.</exception>
    public async Task<IActionResult> OnGetAsync(string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        ReturnUrl = returnUrl;

        return Page();
    }

    /// <summary>Signs in with the recovery code.</summary>
    /// <param name="returnUrl">The local URL to open after sign-in. <see langword="null"/> becomes the site root.</param>
    /// <returns>A local redirect after success, the lockout page, or this page when the code is rejected or the model is invalid.</returns>
    /// <exception cref="InvalidOperationException">No two-factor user is in progress.</exception>
    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        string recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
            return LocalRedirect(returnUrl ?? Url.Content("~/"));
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");
            return RedirectToPage("./Lockout");
        }
        else
        {
            _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            return Page();
        }
    }
}