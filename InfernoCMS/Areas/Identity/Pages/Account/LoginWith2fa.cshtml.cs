// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Completes sign-in with an authenticator code after the password has been accepted.</summary>
public class LoginWith2faModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<LoginWith2faModel> _logger;

    /// <summary>Creates the page with the sign-in services.</summary>
    /// <param name="signInManager">Loads the two-factor user and checks the authenticator code.</param>
    /// <param name="userManager">The user manager. This page does not call it.</param>
    /// <param name="logger">Records success, lockout, and a rejected code.</param>
    public LoginWith2faModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<LoginWith2faModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>The authenticator code and remember-machine choice posted by the form.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>Whether the sign-in cookie should persist. Copied from the password step.</summary>
    public bool RememberMe { get; set; }

    /// <summary>The URL to open after sign-in. May be <see langword="null"/> until the form is posted.</summary>
    public string ReturnUrl { get; set; }

    /// <summary>The fields posted by the authenticator form.</summary>
    public class InputModel
    {
        /// <summary>The code from the authenticator app. Spaces and hyphens are removed before it is checked.</summary>
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        [Display(Name = "Authenticator code")]
        public string TwoFactorCode { get; set; }

        /// <summary>Whether this browser should skip authenticator prompts later.</summary>
        [Display(Name = "Remember this machine")]
        public bool RememberMachine { get; set; }
    }

    /// <summary>Shows the authenticator form for the user who passed the password check.</summary>
    /// <param name="rememberMe">Whether the sign-in cookie should persist.</param>
    /// <param name="returnUrl">The URL to open after sign-in. May be <see langword="null"/>.</param>
    /// <returns>This page.</returns>
    /// <exception cref="InvalidOperationException">No two-factor user is in progress.</exception>
    public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        ReturnUrl = returnUrl;
        RememberMe = rememberMe;

        return Page();
    }

    /// <summary>Signs in with the authenticator code.</summary>
    /// <param name="rememberMe">Whether the sign-in cookie should persist.</param>
    /// <param name="returnUrl">The local URL to open after sign-in. <see langword="null"/> becomes the site root.</param>
    /// <returns>A local redirect after success, the lockout page, or this page when the code is rejected or the model is invalid.</returns>
    /// <exception cref="InvalidOperationException">No two-factor user is in progress.</exception>
    public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        returnUrl ??= Url.Content("~/");

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new InvalidOperationException($"Unable to load two-factor authentication user.");
        }

        string authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, Input.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
            return LocalRedirect(returnUrl);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
            return RedirectToPage("./Lockout");
        }
        else
        {
            _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return Page();
        }
    }
}