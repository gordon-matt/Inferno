// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Shown when sign-in is refused because the account is locked.</summary>
[AllowAnonymous]
public class LockoutModel : PageModel
{
    public void OnGet()
    {
    }
}