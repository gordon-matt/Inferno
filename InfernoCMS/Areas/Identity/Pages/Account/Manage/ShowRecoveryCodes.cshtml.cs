// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Shows recovery codes that were just generated.</summary>
public class ShowRecoveryCodesModel : PageModel
{
    /// <summary>The recovery codes carried from the page that generated them.</summary>
    [TempData]
    public string[] RecoveryCodes { get; set; }

    /// <summary>A message from the page that generated the codes.</summary>
    [TempData]
    public string StatusMessage { get; set; }

    /// <summary>Shows the codes, or returns to the two-factor page when none were carried here.</summary>
    /// <returns>This page when <see cref="RecoveryCodes"/> has entries. Otherwise a redirect to the two-factor page.</returns>
    public IActionResult OnGet() => RecoveryCodes == null || RecoveryCodes.Length == 0 ? RedirectToPage("./TwoFactorAuthentication") : Page();
}