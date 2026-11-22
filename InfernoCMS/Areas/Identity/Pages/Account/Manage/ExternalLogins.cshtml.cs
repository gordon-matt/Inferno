// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Lists, links, and removes external logins for the signed-in user.</summary>
public class ExternalLoginsModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserStore<ApplicationUser> _userStore;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Loads logins and adds or removes a link.</param>
    /// <param name="signInManager">Lists schemes, challenges a provider, and refreshes the sign-in.</param>
    /// <param name="userStore">Reads the password hash that decides whether a login can be removed.</param>
    public ExternalLoginsModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserStore<ApplicationUser> userStore)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userStore = userStore;
    }

    /// <summary>External logins already linked to the account.</summary>
    public IList<UserLoginInfo> CurrentLogins { get; set; }

    /// <summary>Authentication schemes that are not yet linked.</summary>
    public IList<AuthenticationScheme> OtherLogins { get; set; }

    /// <summary>Whether a linked login can be removed. True when the account has a password hash or more than one login.</summary>
    public bool ShowRemoveButton { get; set; }

    /// <summary>A message from the previous link or removal, shown on the next request.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>Loads the linked logins and the schemes that are still available.</summary>
    /// <returns>This page, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        CurrentLogins = await _userManager.GetLoginsAsync(user);
        OtherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToList();

        string passwordHash = null;
        if (_userStore is IUserPasswordStore<ApplicationUser> userPasswordStore)
        {
            passwordHash = await userPasswordStore.GetPasswordHashAsync(user, HttpContext.RequestAborted);
        }

        ShowRemoveButton = passwordHash != null || CurrentLogins.Count > 1;
        return Page();
    }

    /// <summary>Removes the external login and refreshes the sign-in.</summary>
    /// <param name="loginProvider">The authentication scheme name.</param>
    /// <param name="providerKey">The provider's user key.</param>
    /// <returns>A redirect back to this page. Not found when the user is gone.</returns>
    public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
        if (!result.Succeeded)
        {
            StatusMessage = "The external login was not removed.";
            return RedirectToPage();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "The external login was removed.";
        return RedirectToPage();
    }

    /// <summary>Clears the external cookie and challenges <paramref name="provider"/> to link it to the signed-in user.</summary>
    /// <param name="provider">The authentication scheme name.</param>
    /// <returns>The challenge result for <paramref name="provider"/>.</returns>
    public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Request a redirect to the external login provider to link a login for the current user
        string redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
        return new ChallengeResult(provider, properties);
    }

    /// <summary>Stores the external login returned by the provider and clears the external cookie.</summary>
    /// <returns>A redirect back to this page. Not found when the user is gone.</returns>
    /// <exception cref="InvalidOperationException">The provider returned no login information.</exception>
    public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        string userId = await _userManager.GetUserIdAsync(user);
        var info = await _signInManager.GetExternalLoginInfoAsync(userId);
        if (info == null)
        {
            throw new InvalidOperationException($"Unexpected error occurred loading external login info.");
        }

        var result = await _userManager.AddLoginAsync(user, info);
        if (!result.Succeeded)
        {
            StatusMessage = "The external login was not added. External logins can only be associated with one account.";
            return RedirectToPage();
        }

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        StatusMessage = "The external login was added.";
        return RedirectToPage();
    }
}