// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Shows the signed-in user's name and saves a changed phone number.</summary>
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Loads the user and saves the phone number.</param>
    /// <param name="signInManager">Refreshes the sign-in after a successful save.</param>
    public IndexModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>The account's sign-in name.</summary>
    public string Username { get; set; }

    /// <summary>A message from the previous save, shown on the next request.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>The phone number posted by the form.</summary>
    [BindProperty]
    public InputModel Input { get; set; }

    /// <summary>The phone number edited on this page.</summary>
    public class InputModel
    {
        /// <summary>The account phone number.</summary>
        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }
    }

    private async Task LoadAsync(ApplicationUser user)
    {
        string userName = await _userManager.GetUserNameAsync(user);
        string phoneNumber = await _userManager.GetPhoneNumberAsync(user);

        Username = userName;

        Input = new InputModel
        {
            PhoneNumber = phoneNumber
        };
    }

    /// <summary>Loads the signed-in user's name and phone number.</summary>
    /// <returns>This page, or not found when the user cannot be loaded.</returns>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await LoadAsync(user);
        return Page();
    }

    /// <summary>Saves the phone number when it changed, then refreshes the sign-in.</summary>
    /// <returns>A redirect back to this page, or this page again when the model is invalid. Not found when the user is gone.</returns>
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync(user);
            return Page();
        }

        string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        if (Input.PhoneNumber != phoneNumber)
        {
            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }
}