// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Signs in with an external provider, or creates a local account when the login is new.</summary>
[AllowAnonymous]
public class ExternalLoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ExternalLoginModel> _logger;

    /// <summary>Creates the page with the external-login services.</summary>
    /// <param name="signInManager">Challenges the provider and signs an existing login in.</param>
    /// <param name="userManager">Creates the local account and the confirmation token.</param>
    /// <param name="userStore">Stores the user name. It must also support email.</param>
    /// <param name="logger">Records an external sign-in and a created account.</param>
    /// <param name="emailSender">Sends the confirmation message for a new account.</param>
    /// <exception cref="NotSupportedException">The user store does not support email.</exception>
    public ExternalLoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ILogger<ExternalLoginModel> logger,
        IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _logger = logger;
        _emailSender = emailSender;
    }

    /// <summary>The email address posted when a new local account is needed.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>The provider name shown on the confirmation form.</summary>
    public string ProviderDisplayName { get; set; }

    /// <summary>The local URL to open after sign-in. A missing value becomes the site root.</summary>
    public string ReturnUrl { get; set; }

    /// <summary>An error from the provider or from a missing login, shown on the login page.</summary>
    [TempData]
    public string ErrorMessage { get; set; }

    /// <summary>The email collected for a new local account.</summary>
    public class InputModel
    {
        /// <summary>The email address. It is also stored as the user name.</summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    /// <summary>Sends a GET request to the login page. External login starts from a POST.</summary>
    /// <returns>A redirect to the login page.</returns>
    public IActionResult OnGet() => RedirectToPage("./Login");

    /// <summary>Challenges <paramref name="provider"/> and returns to the callback handler.</summary>
    /// <param name="provider">The authentication scheme name.</param>
    /// <param name="returnUrl">The URL to carry through the callback. May be <see langword="null"/>.</param>
    /// <returns>The challenge result for <paramref name="provider"/>.</returns>
    public IActionResult OnPost(string provider, string returnUrl = null)
    {
        // Request a redirect to the external login provider.
        string redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    /// <summary>Signs in an existing external login, or shows the confirmation form when the login is new.</summary>
    /// <param name="returnUrl">The local URL to open after sign-in. <see langword="null"/> becomes the site root.</param>
    /// <param name="remoteError">An error reported by the provider. A non-<see langword="null"/> value returns to the login page.</param>
    /// <returns>A redirect after sign-in, lockout, or a provider error. Otherwise the confirmation form.</returns>
    public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
    {
        returnUrl ??= Url.Content("~/");
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
            return LocalRedirect(returnUrl);
        }
        if (result.IsLockedOut)
        {
            return RedirectToPage("./Lockout");
        }
        else
        {
            // If the user does not have an account, then ask the user to create an account.
            ReturnUrl = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName;
            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                Input = new InputModel
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                };
            }
            return Page();
        }
    }

    /// <summary>Creates a local account for the external login, emails a confirmation link, and signs in when confirmation is not required.</summary>
    /// <param name="returnUrl">The local URL to open after sign-in. <see langword="null"/> becomes the site root.</param>
    /// <returns>A redirect after the account is created, or this page when the model is invalid or Identity rejects the account.</returns>
    /// <exception cref="InvalidOperationException"><see cref="ApplicationUser"/> cannot be constructed.</exception>
    public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        // Get the information about the user from the external login provider
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            ErrorMessage = "Error loading external login information during confirmation.";
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        if (ModelState.IsValid)
        {
            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                    string userId = await _userManager.GetUserIdAsync(user);
                    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    string callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    // If account confirmation is required, we need to show the link if we don't have a real email sender
                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("./RegisterConfirmation", new { Input.Email });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ProviderDisplayName = info.ProviderDisplayName;
        ReturnUrl = returnUrl;
        return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore() => !_userManager.SupportsUserEmail
            ? throw new NotSupportedException("The default UI requires a user store with email support.")
            : (IUserEmailStore<ApplicationUser>)_userStore;
}