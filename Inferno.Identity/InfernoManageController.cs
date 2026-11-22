using System.Text;
using System.Text.Encodings.Web;
using Inferno.Identity.Entities;
using Inferno.Identity.Models.ManageViewModels;
using Inferno.Identity.Services;
using Inferno.Web.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Inferno.Identity;

/// <summary>Signed-in account management for <typeparamref name="TUser"/>: profile, password, external logins, and two-factor authentication.</summary>
/// <typeparam name="TUser">The Identity user type.</typeparam>
[Authorize]
public abstract class InfernoManageController<TUser> : Controller
    where TUser : InfernoIdentityUser, new()
{
    private readonly UserManager<TUser> userManager;
    private readonly SignInManager<TUser> signInManager;
    private readonly IEmailSender emailSender;
    private readonly ILogger logger;
    private readonly UrlEncoder urlEncoder;
    private readonly SiteSettings siteSettings;

    private const string AuthenicatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    /// <summary>Stores the Identity services used by the management actions.</summary>
    /// <param name="userManager">Loads and updates the signed-in user.</param>
    /// <param name="signInManager">Refreshes the sign-in and lists external schemes.</param>
    /// <param name="emailSender">Sends the verification email.</param>
    /// <param name="logger">Records password and two-factor changes.</param>
    /// <param name="urlEncoder">Encodes the site name and email in the authenticator URI.</param>
    /// <param name="siteSettings">Supplies the site name used as the authenticator issuer.</param>
    public InfernoManageController(
      UserManager<TUser> userManager,
      SignInManager<TUser> signInManager,
      IEmailSender emailSender,
      ILogger<InfernoManageController<TUser>> logger,
      UrlEncoder urlEncoder,
      SiteSettings siteSettings)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.emailSender = emailSender;
        this.logger = logger;
        this.urlEncoder = urlEncoder;
        this.siteSettings = siteSettings;
    }

    /// <summary>A message from the previous management action, shown on the next page.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>Shows the signed-in user's name, email, phone number, and confirmation state.</summary>
    /// <returns>The profile view.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var model = new IndexViewModel
        {
            Username = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsEmailConfirmed = user.EmailConfirmed,
            StatusMessage = StatusMessage
        };

        return View(model);
    }

    /// <summary>Saves a changed email or phone number, then returns to the profile page.</summary>
    /// <param name="model">The posted email and phone number.</param>
    /// <returns>A redirect to the profile page, or the form again when the model is invalid.</returns>
    /// <exception cref="ApplicationException">The user cannot be loaded, or Identity rejects the email or phone update.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Index(IndexViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        string email = user.Email;
        if (model.Email != email)
        {
            var setEmailResult = await userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
            }
        }

        string phoneNumber = user.PhoneNumber;
        if (model.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
            }
        }

        StatusMessage = "Your profile has been updated";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Emails an address-confirmation link to the signed-in user.</summary>
    /// <param name="model">The posted profile. It is not used when the model is valid.</param>
    /// <returns>A redirect to the profile page, or the form again when the model is invalid.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> SendVerificationEmail(IndexViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        string code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        string callbackUrl = Url.EmailConfirmationLink<TUser>(user.Id, code, Request.Scheme);
        string email = user.Email;
        await emailSender.SendEmailConfirmationAsync(email, callbackUrl);

        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Shows the change-password form, or the set-password page when the account has no password.</summary>
    /// <returns>The change-password view, or a redirect to set a password.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> ChangePassword()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        bool hasPassword = await userManager.HasPasswordAsync(user);
        if (!hasPassword)
        {
            return RedirectToAction(nameof(SetPassword));
        }

        var model = new ChangePasswordViewModel { StatusMessage = StatusMessage };
        return View(model);
    }

    /// <summary>Changes the password, then signs the user in again.</summary>
    /// <param name="model">The current password and the replacement.</param>
    /// <returns>A redirect back to this page after success, or the form with the Identity errors.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            AddErrors(changePasswordResult);
            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        logger.LogInformation("User changed their password successfully.");
        StatusMessage = "Your password has been changed.";

        return RedirectToAction(nameof(ChangePassword));
    }

    /// <summary>Shows the set-password form, or the change-password page when the account already has a password.</summary>
    /// <returns>The set-password view, or a redirect to change the password.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> SetPassword()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        bool hasPassword = await userManager.HasPasswordAsync(user);

        if (hasPassword)
        {
            return RedirectToAction(nameof(ChangePassword));
        }

        var model = new SetPasswordViewModel { StatusMessage = StatusMessage };
        return View(model);
    }

    /// <summary>Adds a password to an account that does not have one, then signs the user in again.</summary>
    /// <param name="model">The password to add.</param>
    /// <returns>A redirect back to this page after success, or the form with the Identity errors.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var addPasswordResult = await userManager.AddPasswordAsync(user, model.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            AddErrors(addPasswordResult);
            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        StatusMessage = "Your password has been set.";

        return RedirectToAction(nameof(SetPassword));
    }

    /// <summary>Lists the external logins already linked and the schemes that are still available.</summary>
    /// <returns>The external-logins view. Remove is offered when the account has a password or more than one login.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> ExternalLogins()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var model = new ExternalLoginsViewModel { CurrentLogins = await userManager.GetLoginsAsync(user) };
        model.OtherLogins = (await signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(auth => model.CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToList();
        model.ShowRemoveButton = await userManager.HasPasswordAsync(user) || model.CurrentLogins.Count > 1;
        model.StatusMessage = StatusMessage;

        return View(model);
    }

    /// <summary>Clears the external cookie and challenges <paramref name="provider"/> to link it to the signed-in user.</summary>
    /// <param name="provider">The authentication scheme name.</param>
    /// <returns>The challenge result for <paramref name="provider"/>.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> LinkLogin(string provider)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Request a redirect to the external login provider to link a login for the current user
        string redirectUrl = Url.Action(nameof(LinkLoginCallback));
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userManager.GetUserId(User));
        return new ChallengeResult(provider, properties);
    }

    /// <summary>Stores the external login returned by the provider and clears the external cookie.</summary>
    /// <returns>A redirect to the external-logins page.</returns>
    /// <exception cref="ApplicationException">The user cannot be loaded, the provider returned no login, or Identity rejects the link.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> LinkLoginCallback()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var info = await signInManager.GetExternalLoginInfoAsync(user.Id);
        if (info == null)
        {
            throw new ApplicationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
        }

        var result = await userManager.AddLoginAsync(user, info);
        if (!result.Succeeded)
        {
            throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
        }

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        StatusMessage = "The external login was added.";
        return RedirectToAction(nameof(ExternalLogins));
    }

    /// <summary>Unlinks the posted external login and signs the user in again.</summary>
    /// <param name="model">The scheme name and provider key to remove.</param>
    /// <returns>A redirect to the external-logins page.</returns>
    /// <exception cref="ApplicationException">The user cannot be loaded, or Identity rejects the removal.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> RemoveLogin(RemoveLoginViewModel model)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var result = await userManager.RemoveLoginAsync(user, model.LoginProvider, model.ProviderKey);
        if (!result.Succeeded)
        {
            throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        StatusMessage = "The external login was removed.";
        return RedirectToAction(nameof(ExternalLogins));
    }

    /// <summary>Shows whether an authenticator is configured, whether two-factor is on, and how many recovery codes remain.</summary>
    /// <returns>The two-factor view.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> TwoFactorAuthentication()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var model = new TwoFactorAuthenticationViewModel
        {
            HasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user) != null,
            Is2faEnabled = user.TwoFactorEnabled,
            RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user),
        };

        return View(model);
    }

    /// <summary>Shows the warning before two-factor authentication is turned off.</summary>
    /// <returns>The disable warning view.</returns>
    /// <exception cref="ApplicationException">The user cannot be loaded, or two-factor authentication is already off.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> Disable2faWarning()
    {
        var user = await userManager.GetUserAsync(User);
        return user == null
            ? throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.")
            : !user.TwoFactorEnabled
            ? throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.")
            : (IActionResult)View(nameof(Disable2fa));
    }

    /// <summary>Turns two-factor authentication off for the signed-in user.</summary>
    /// <returns>A redirect to the two-factor page.</returns>
    /// <exception cref="ApplicationException">The user cannot be loaded, or Identity rejects the change.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Disable2fa()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
        }

        logger.LogInformation("User with ID {UserId} has disabled 2fa.", user.Id);
        return RedirectToAction(nameof(TwoFactorAuthentication));
    }

    /// <summary>Shows the authenticator key and QR code. A missing key is reset before it is shown.</summary>
    /// <returns>The authenticator setup view.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> EnableAuthenticator()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        string unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var model = new EnableAuthenticatorViewModel
        {
            SharedKey = FormatKey(unformattedKey),
            AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey)
        };

        return View(model);
    }

    /// <summary>Turns two-factor authentication on when the verification code matches. Spaces and hyphens are removed first.</summary>
    /// <param name="model">The verification code from the authenticator app.</param>
    /// <returns>A redirect to the recovery-code page after success, or the form when the code is invalid.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        // Strip spaces and hypens
        string verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        bool is2faTokenValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError("model.Code", "Verification code is invalid.");
            return View(model);
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        logger.LogInformation("User with ID {UserId} has enabled 2FA with an authenticator app.", user.Id);
        return RedirectToAction(nameof(GenerateRecoveryCodes));
    }

    /// <summary>Shows the warning before the authenticator key is reset.</summary>
    /// <returns>The reset warning view.</returns>
    [HttpGet]
    public virtual IActionResult ResetAuthenticatorWarning() => View(nameof(ResetAuthenticator));

    /// <summary>Turns two-factor authentication off and replaces the authenticator key.</summary>
    /// <returns>A redirect to authenticator setup.</returns>
    /// <exception cref="ApplicationException">The signed-in user cannot be loaded.</exception>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> ResetAuthenticator()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        logger.LogInformation("User with id '{UserId}' has reset their authentication app key.", user.Id);

        return RedirectToAction(nameof(EnableAuthenticator));
    }

    /// <summary>Replaces the recovery codes with ten new ones.</summary>
    /// <returns>The recovery-code view.</returns>
    /// <exception cref="ApplicationException">The user cannot be loaded, or two-factor authentication is off.</exception>
    [HttpGet]
    public virtual async Task<IActionResult> GenerateRecoveryCodes()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        if (!user.TwoFactorEnabled)
        {
            throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
        }

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        var model = new GenerateRecoveryCodesViewModel { RecoveryCodes = recoveryCodes.ToArray() };

        logger.LogInformation("User with ID {UserId} has generated new 2FA recovery codes.", user.Id);

        return View(model);
    }

    #region Helpers

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey[currentPosition..]);
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey) => string.Format(
            AuthenicatorUriFormat,
            urlEncoder.Encode(siteSettings.SiteName),
            urlEncoder.Encode(email),
            unformattedKey);

    #endregion Helpers
}