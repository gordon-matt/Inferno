// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Areas.Identity.Pages.Account;

/// <summary>Shown when the signed-in user is not allowed to open a page.</summary>
public class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
    }
}