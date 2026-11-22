// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Shows the download and delete actions for the signed-in user's data.</summary>
public class PersonalDataModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PersonalDataModel> _logger;

    /// <summary>Creates the page with the account services.</summary>
    /// <param name="userManager">Confirms that the signed-in user still exists.</param>
    /// <param name="logger">The page logger. This page does not write to it.</param>
    public PersonalDataModel(
        UserManager<ApplicationUser> userManager,
        ILogger<PersonalDataModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>Shows the page when the signed-in user can be loaded.</summary>
    /// <returns>This page, or not found when the user is gone.</returns>
    public async Task<IActionResult> OnGet()
    {
        var user = await _userManager.GetUserAsync(User);
        return user == null ? NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.") : Page();
    }
}