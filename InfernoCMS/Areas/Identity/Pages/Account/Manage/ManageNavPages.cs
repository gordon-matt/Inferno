// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InfernoCMS.Areas.Identity.Pages.Account.Manage;

/// <summary>Page names and active-link classes for the account-management navigation.</summary>
public static class ManageNavPages
{
    /// <summary>The profile page name.</summary>
    public static string Index => "Index";

    /// <summary>The email page name.</summary>
    public static string Email => "Email";

    /// <summary>The change-password page name.</summary>
    public static string ChangePassword => "ChangePassword";

    /// <summary>The download page name.</summary>
    public static string DownloadPersonalData => "DownloadPersonalData";

    /// <summary>The delete page name.</summary>
    public static string DeletePersonalData => "DeletePersonalData";

    /// <summary>The external-logins page name.</summary>
    public static string ExternalLogins => "ExternalLogins";

    /// <summary>The personal-data page name.</summary>
    public static string PersonalData => "PersonalData";

    /// <summary>The two-factor page name.</summary>
    public static string TwoFactorAuthentication => "TwoFactorAuthentication";

    /// <summary>The CSS class for the profile link.</summary>
    /// <param name="viewContext">The current view.</param>
    /// <returns>The text active when the profile page is current. Otherwise <see langword="null"/>.</returns>
    public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

    /// <summary>The CSS class for the email link.</summary>
    /// <param name="viewContext">The current view.</param>
    /// <returns>The text active when the email page is current. Otherwise <see langword="null"/>.</returns>
    public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

    /// <summary>The CSS class for the change-password link.</summary>
    /// <param name="viewContext">The current view.</param>
    /// <returns>The text active when the change-password page is current. Otherwise <see langword="null"/>.</returns>
    public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

    /// <summary>The CSS class for the download link.</summary>
    /// <param name="viewContext">The current view.</param>
    /// <returns>The text active when the download page is current. Otherwise <see langword="null"/>.</returns>
    public static string DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);

    /// <summary>The CSS class for the delete link.</summary>
    /// <param name="viewContext">The current view.</param>
    /// <returns>The text active when the delete page is current. Otherwise <see langword="null"/>.</returns>
    public static string DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);

    /// <summary>The CSS class for the external-logins link.</summary>
    /// <param name="viewContext">The current view.</param>
    /// <returns>The text active when the external-logins page is current. Otherwise <see langword="null"/>.</returns>
    public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

    /// <summary>The CSS class for the personal-data link.</summary>
    /// <param name="viewContext">The current view.</param>
    /// <returns>The text active when the personal-data page is current. Otherwise <see langword="null"/>.</returns>
    public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

    /// <summary>The CSS class for the two-factor link.</summary>
    /// <param name="viewContext">The current view.</param>
    /// <returns>The text active when the two-factor page is current. Otherwise <see langword="null"/>.</returns>
    public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);

    /// <summary>The CSS class for one management link.</summary>
    /// <param name="viewContext">The current view. Its ActivePage value wins over the page file name.</param>
    /// <param name="page">The page name to compare.</param>
    /// <returns>The text active when <paramref name="page"/> is the current page. Otherwise <see langword="null"/>.</returns>
    public static string PageNavClass(ViewContext viewContext, string page)
    {
        string activePage = viewContext.ViewData["ActivePage"] as string
            ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
    }
}