//TODO: Review all of this and replace some code with membershipService, same as in Inferno

using System.Security.Claims;
using Inferno.Identity.Entities;
using Inferno.Identity.Models.AccountViewModels;
using Inferno.Identity.Services;
using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Web;
using Inferno.Web.Mvc;
using Inferno.Web.Mvc.Routing;
using Inferno.Web.Security.Membership;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Inferno.Identity;

/// <summary>Account sign-in, registration, password reset, external login, and profile pages for <typeparamref name="TUser"/>.</summary>
/// <typeparam name="TUser">The Identity user type.</typeparam>
[Authorize]
public abstract class InfernoAccountController<TUser> : InfernoController
    where TUser : InfernoIdentityUser, new()
{
    private readonly UserManager<TUser> _userManager;
    private readonly SignInManager<TUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger _logger;
    private readonly IMembershipService membershipService;
    private readonly Lazy<IEnumerable<IUserProfileProvider>> userProfileProviders;

    /// <summary>Stores the Identity services used by the account actions.</summary>
    /// <param name="userManager">Creates users and confirms email and passwords.</param>
    /// <param name="signInManager">Signs users in and out, including external and two-factor sign-in.</param>
    /// <param name="emailSender">Sends confirmation and password-reset mail.</param>
    /// <param name="logger">Records sign-in, lockout, and registration.</param>
    /// <param name="membershipService">Loads users shown on the profile pages.</param>
    /// <param name="userProfileProviders">Supplies the profile fields posted by the update form.</param>
    public InfernoAccountController(
        UserManager<TUser> userManager,
        SignInManager<TUser> signInManager,
        IEmailSender emailSender,
        ILogger<InfernoAccountController<TUser>> logger,
        IMembershipService membershipService,
        Lazy<IEnumerable<IUserProfileProvider>> userProfileProviders)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _logger = logger;
        this.membershipService = membershipService;
        this.userProfileProviders = userProfileProviders;
    }

    /// <summary>An error from an external login, shown on the next login page.</summary>
    [TempData]
    public string ErrorMessage { get; set; }

    /// <summary>Clears the external-login cookie and shows the login form.</summary>
    /// <param name="returnUrl">The URL to open after sign-in. May be <see langword="null"/>.</param>
    /// <returns>The login view.</returns>
    [HttpGet]
    [AllowAnonymous]
    public virtual async Task<IActionResult> Login(string returnUrl = null)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>Signs in with the posted email and password. A failed password does not count toward lockout.</summary>
    /// <param name="model">The posted email, password, and remember-me choice.</param>
    /// <param name="returnUrl">The local URL to open after sign-in. A non-local or <see langword="null"/> value opens the home page.</param>
    /// <returns>A redirect after success, two-factor, or lockout. Otherwise the login view with the validation errors.</returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToLocal(returnUrl);
            }
            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(LoginWith2fa), new { returnUrl, model.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToAction(nameof(Lockout));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    /// <summary>Shows the authenticator-code form for a user who has already passed the password check.</summary>
    /// <param name="rememberMe">Whether the sign-in cookie should persist. Copied onto the form.</param>
    /// <param name="returnUrl">The URL to open after sign-in. May be <see langword="null"/>.</param>
    /// <returns>The authenticator form.</returns>
    /// <exception cref="ApplicationException">No two-factor user is in progress.</exception>
    [HttpGet]
    [AllowAnonymous]
    public virtual async Task<IActionResult> LoginWith2fa(bool rememberMe, string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            throw new ApplicationException($"Unable to load two-factor authentication user.");
        }

        var model = new LoginWith2faViewModel { RememberMe = rememberMe };
        ViewData["ReturnUrl"] = returnUrl;

        return View(model);
    }

    /// <summary>Signs in with the authenticator code. Spaces and hyphens are removed before it is checked.</summary>
    /// <param name="model">The authenticator code and remember-machine choice.</param>
    /// <param name="rememberMe">Whether the sign-in cookie should persist.</param>
    /// <param name="returnUrl">The local URL to open after sign-in. A non-local or <see langword="null"/> value opens the home page.</param>
    /// <returns>A redirect after success or lockout. Otherwise the form, including when the model is invalid.</returns>
    /// <exception cref="ApplicationException">No two-factor user is in progress.</exception>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> LoginWith2fa(LoginWith2faViewModel model, bool rememberMe, string returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        string authenticatorCode = model.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, rememberMe, model.RememberMachine);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID {UserId} logged in with 2fa.", user.Id);
            return RedirectToLocal(returnUrl);
        }
        else if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            _logger.LogWarning("Invalid authenticator code entered for user with ID {UserId}.", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
            return View();
        }
    }

    /// <summary>Shows the recovery-code form for a user who has already passed the password check.</summary>
    /// <param name="returnUrl">The URL to open after sign-in. May be <see langword="null"/>.</param>
    /// <returns>The recovery-code form.</returns>
    /// <exception cref="ApplicationException">No two-factor user is in progress.</exception>
    [HttpGet]
    [AllowAnonymous]
    public virtual async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new ApplicationException($"Unable to load two-factor authentication user.");
        }

        ViewData["ReturnUrl"] = returnUrl;

        return View();
    }

    /// <summary>Signs in with a recovery code. Spaces are removed before it is checked.</summary>
    /// <param name="model">The posted recovery code.</param>
    /// <param name="returnUrl">The local URL to open after sign-in. A non-local or <see langword="null"/> value opens the home page.</param>
    /// <returns>A redirect after success or lockout. Otherwise the form, including when the model is invalid.</returns>
    /// <exception cref="ApplicationException">No two-factor user is in progress.</exception>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model, string returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new ApplicationException($"Unable to load two-factor authentication user.");
        }

        string recoveryCode = model.RecoveryCode.Replace(" ", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result.Succeeded)
        {
            _logger.LogInformation("User with ID {UserId} logged in with a recovery code.", user.Id);
            return RedirectToLocal(returnUrl);
        }
        if (result.IsLockedOut)
        {
            _logger.LogWarning("User with ID {UserId} account locked out.", user.Id);
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            _logger.LogWarning("Invalid recovery code entered for user with ID {UserId}", user.Id);
            ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
            return View();
        }
    }

    /// <summary>Shows the lockout page.</summary>
    /// <returns>The lockout view.</returns>
    [HttpGet]
    [AllowAnonymous]
    public virtual IActionResult Lockout() => View();

    /// <summary>Shows the registration form.</summary>
    /// <param name="returnUrl">The URL to open after registration. May be <see langword="null"/>.</param>
    /// <returns>The registration view.</returns>
    [HttpGet]
    [AllowAnonymous]
    public virtual IActionResult Register(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Creates an account from the posted email, sends a confirmation link, and signs the user in.
    /// The email is stored as both the user name and the email address.
    /// </summary>
    /// <param name="model">The posted email and password.</param>
    /// <param name="returnUrl">The local URL to open after sign-in. A non-local or <see langword="null"/> value opens the home page.</param>
    /// <returns>A redirect after the account is created. Otherwise the registration view with the Identity errors.</returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (ModelState.IsValid)
        {
            var user = new TUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string callbackUrl = Url.EmailConfirmationLink<TUser>(user.Id, code, Request.Scheme);
                await _emailSender.SendEmailConfirmationAsync(model.Email, callbackUrl);

                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User created a new account with password.");
                return RedirectToLocal(returnUrl);
            }
            AddErrors(result);
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    /// <summary>Signs the current user out and opens the home page.</summary>
    /// <returns>A redirect to the home page.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return RedirectToAction("Index", "Home");
    }

    /// <summary>Challenges <paramref name="provider"/> and returns to the external-login callback.</summary>
    /// <param name="provider">The authentication scheme name.</param>
    /// <param name="returnUrl">The URL to carry through the callback. May be <see langword="null"/>.</param>
    /// <returns>The challenge result for <paramref name="provider"/>.</returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public virtual IActionResult ExternalLogin(string provider, string returnUrl = null)
    {
        // Request a redirect to the external login provider.
        string redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    /// <summary>Signs in an existing external login, or shows the confirmation form when the login is new.</summary>
    /// <param name="returnUrl">The local URL to open after sign-in. A non-local or <see langword="null"/> value opens the home page.</param>
    /// <param name="remoteError">An error reported by the provider. A non-<see langword="null"/> value returns to the login page.</param>
    /// <returns>A redirect after sign-in, lockout, or a provider error. Otherwise the external-login confirmation view.</returns>
    [HttpGet]
    [AllowAnonymous]
    public virtual async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return RedirectToAction(nameof(Login));
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
            return RedirectToLocal(returnUrl);
        }
        if (result.IsLockedOut)
        {
            return RedirectToAction(nameof(Lockout));
        }
        else
        {
            // If the user does not have an account, then ask the user to create an account.
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;
            string email = info.Principal.FindFirstValue(ClaimTypes.Email);
            return View("ExternalLogin", new ExternalLoginViewModel { Email = email });
        }
    }

    /// <summary>Creates a local account for the external login and signs it in.</summary>
    /// <param name="model">The email address for the new account.</param>
    /// <param name="returnUrl">The local URL to open after sign-in. A non-local or <see langword="null"/> value opens the home page.</param>
    /// <returns>A redirect after the account is created. Otherwise the confirmation view with the Identity errors.</returns>
    /// <exception cref="ApplicationException">The external login information is no longer available.</exception>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                throw new ApplicationException("Error loading external login information during confirmation.");
            }
            var user = new TUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                    return RedirectToLocal(returnUrl);
                }
            }
            AddErrors(result);
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(nameof(ExternalLogin), model);
    }

    /// <summary>Confirms the email address for <paramref name="userId"/> with <paramref name="code"/>.</summary>
    /// <param name="userId">The user id. A <see langword="null"/> value opens the home page.</param>
    /// <param name="code">The confirmation token. A <see langword="null"/> value opens the home page.</param>
    /// <returns>The confirmation view when the token is accepted, the error view when it is not, or a redirect home when either value is missing.</returns>
    /// <exception cref="ApplicationException">No user has <paramref name="userId"/>.</exception>
    [HttpGet]
    [AllowAnonymous]
    public virtual async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction("Index", "Home");
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ApplicationException($"Unable to load user with ID '{userId}'.");
        }
        var result = await _userManager.ConfirmEmailAsync(user, code);
        return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    /// <summary>Shows the forgot-password form.</summary>
    /// <returns>The forgot-password view.</returns>
    [HttpGet]
    [AllowAnonymous]
    public virtual IActionResult ForgotPassword() => View();

    /// <summary>
    /// Emails a reset link when the address belongs to a confirmed user.
    /// A missing or unconfirmed account is sent to the same confirmation page.
    /// </summary>
    /// <param name="model">The posted email address.</param>
    /// <returns>The confirmation page, or the form again when the model is invalid.</returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            string code = await _userManager.GeneratePasswordResetTokenAsync(user);
            string callbackUrl = Url.ResetPasswordCallbackLink<TUser>(user.Id, code, Request.Scheme);
            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
               $"Please reset your password by clicking here: <a href='{callbackUrl}'>link</a>");
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    /// <summary>Shows the page that follows a reset request.</summary>
    /// <returns>The confirmation view.</returns>
    [HttpGet]
    [AllowAnonymous]
    public virtual IActionResult ForgotPasswordConfirmation() => View();

    /// <summary>Shows the reset form for <paramref name="code"/>.</summary>
    /// <param name="code">The password-reset token. It is copied onto the form.</param>
    /// <returns>The reset view.</returns>
    /// <exception cref="ApplicationException"><paramref name="code"/> is <see langword="null"/>.</exception>
    [HttpGet]
    [AllowAnonymous]
    public virtual IActionResult ResetPassword(string code = null)
    {
        if (code == null)
        {
            throw new ApplicationException("A code must be supplied for password reset.");
        }
        var model = new ResetPasswordViewModel { Code = code };
        return View(model);
    }

    /// <summary>Resets the password for the posted email. A missing account is sent to the confirmation page.</summary>
    /// <param name="model">The email, new password, and reset token.</param>
    /// <returns>The confirmation page after success or a missing account. Otherwise the form with the Identity errors.</returns>
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        AddErrors(result);
        return View();
    }

    /// <summary>Shows the page that follows a password reset.</summary>
    /// <returns>The confirmation view.</returns>
    [HttpGet]
    [AllowAnonymous]
    public virtual IActionResult ResetPasswordConfirmation() => View();

    /// <summary>Shows the access-denied page.</summary>
    /// <returns>The access-denied view.</returns>
    [HttpGet]
    public virtual IActionResult AccessDenied() => View();

    #region User Profile

    /// <summary>
    /// Shows the profile for <paramref name="userId"/>.
    /// The current user and a caller with full access can edit it. Anyone else can view it.
    /// </summary>
    /// <param name="userId">The user id to show.</param>
    /// <returns>The profile view.</returns>
    public virtual async Task<ActionResult> ViewProfile(string userId)
    {
        WorkContext.Value.Breadcrumbs.Add(T[InfernoIdentityLocalizableStrings.Title].Value);

        if (userId == WorkContext.Value.CurrentUser.Id)
        {
            ViewBag.Title = T[InfernoIdentityLocalizableStrings.MyProfile].Value;
            WorkContext.Value.Breadcrumbs.Add(T[InfernoIdentityLocalizableStrings.MyProfile].Value);
            ViewBag.CanEdit = true;
        }
        else if (await AuthorizeAsync(StandardPolicies.FullAccess))
        {
            var user = await membershipService.GetUserByIdAsync(userId);
            ViewBag.Title = string.Format(T[InfernoIdentityLocalizableStrings.ProfileForUser].Value, user.UserName);
            WorkContext.Value.Breadcrumbs.Add(string.Format(T[InfernoIdentityLocalizableStrings.ProfileForUser].Value, user.UserName));
            ViewBag.CanEdit = true;
        }
        else
        {
            var user = await membershipService.GetUserByIdAsync(userId);
            ViewBag.Title = string.Format(T[InfernoIdentityLocalizableStrings.ProfileForUser].Value, user.UserName);
            WorkContext.Value.Breadcrumbs.Add(string.Format(T[InfernoIdentityLocalizableStrings.ProfileForUser].Value, user.UserName));
            ViewBag.CanEdit = false;
        }

        return View("Profile", model: userId);
    }

    /// <summary>Shows the signed-in user's profile.</summary>
    /// <returns>The profile view for the current user.</returns>
    public virtual async Task<ActionResult> ViewMyProfile() => await ViewProfile(WorkContext.Value.CurrentUser.Id);

    /// <summary>Shows the profile editor for the current user, or for <paramref name="userId"/> when the caller has full access.</summary>
    /// <param name="userId">The user id to edit.</param>
    /// <returns>The editor, or an unauthorized result when the caller is neither that user nor a full-access caller.</returns>
    public virtual async Task<ActionResult> EditProfile(string userId)
    {
        WorkContext.Value.Breadcrumbs.Add(T[InfernoIdentityLocalizableStrings.Title].Value);

        if (userId == WorkContext.Value.CurrentUser.Id)
        {
            ViewBag.Title = T[InfernoIdentityLocalizableStrings.EditMyProfile].Value;
            WorkContext.Value.Breadcrumbs.Add(T[InfernoIdentityLocalizableStrings.MyProfile].Value, Url.Action("ViewMyProfile"));
            WorkContext.Value.Breadcrumbs.Add(T[InfernoWebLocalizableStrings.General.Edit].Value);
        }
        else if (await AuthorizeAsync(StandardPolicies.FullAccess))
        {
            ViewBag.Title = T[InfernoIdentityLocalizableStrings.EditProfile].Value;
            var user = await membershipService.GetUserByIdAsync(userId);
            WorkContext.Value.Breadcrumbs.Add(string.Format(T[InfernoIdentityLocalizableStrings.ProfileForUser].Value, user.UserName), Url.Action("ViewProfile", new { userId }));
            WorkContext.Value.Breadcrumbs.Add(T[InfernoWebLocalizableStrings.General.Edit].Value);
        }
        else
        {
            return Unauthorized();
        }

        return View("ProfileEdit", model: userId);
    }

    /// <summary>Shows the editor for the signed-in user's profile.</summary>
    /// <returns>The profile editor for the current user.</returns>
    public virtual async Task<ActionResult> EditMyProfile() => await EditProfile(WorkContext.Value.CurrentUser.Id);

    /// <summary>
    /// Saves the posted profile fields. A checkbox value of true,false is stored as true.
    /// </summary>
    /// <returns>A redirect to the current user's profile, carrying another user's id when the form edited someone else.</returns>
    [HttpPost]
    public virtual async Task<ActionResult> UpdateProfile()
    {
        var userId = Request.Form["UserId"];

        var newProfile = new Dictionary<string, string>();

        foreach (var provider in userProfileProviders.Value)
        {
            foreach (string fieldName in provider.GetFieldNames())
            {
                string value = Request.Form[fieldName];

                if (value == "true,false")
                {
                    value = "true";
                }

                newProfile.Add(fieldName, value);
            }
        }

        await membershipService.UpdateProfileAsync(userId, newProfile);

        //eventBus.Notify<IMembershipEventHandler>(x => x.ProfileChanged(userId, newProfile));

        return userId == WorkContext.Value.CurrentUser.Id
            ? RedirectToAction("ViewMyProfile")
            : RedirectToAction("ViewMyProfile", RouteData.Values.Merge(new { userId }));
    }

    #endregion User Profile

    #region Helpers

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string returnUrl) => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");

    #endregion Helpers
}