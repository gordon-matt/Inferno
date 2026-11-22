// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Signs in with a password. A failed password counts toward lockout.</summary>
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<LoginModel> _logger;

    /// <summary>Creates the page with the sign-in services.</summary>
    /// <param name="signInManager">Checks the password and lists external schemes.</param>
    /// <param name="logger">Records a successful sign-in and a lockout.</param>
    public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger)
    {
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>The email, password, and remember-me choice posted by the form.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>External authentication schemes offered on the form.</summary>
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    /// <summary>The local URL to open after sign-in. A missing value becomes the site root.</summary>
    public string ReturnUrl { get; set; }

    /// <summary>An error carried from another page, such as an external login failure.</summary>
    [TempData]
    public string ErrorMessage { get; set; }

    /// <summary>The fields posted by the login form.</summary>
    public class InputModel
    {
        /// <summary>The account email used to sign in.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>The account password.</summary>
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>Whether the sign-in cookie should persist after the browser closes.</summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    /// <summary>Clears the external-login cookie and prepares the form.</summary>
    /// <param name="returnUrl">The URL to open after sign-in. <see langword="null"/> becomes the site root.</param>
    /// <returns>A task that completes when the external cookie has been cleared and the schemes have been loaded.</returns>
    public async Task OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        ReturnUrl = returnUrl;
    }

    /// <summary>Signs in with the posted password. A failed password counts toward lockout.</summary>
    /// <param name="returnUrl">The local URL to open after sign-in. <see langword="null"/> becomes the site root.</param>
    /// <returns>A local redirect after success, the two-factor page, the lockout page, or this page again when sign-in fails.</returns>
    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // Failed passwords count towards the lockout configured in the Identity options.
            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, Input.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}