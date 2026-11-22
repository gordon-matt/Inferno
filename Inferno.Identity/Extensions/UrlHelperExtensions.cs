using Inferno.Identity;
using Inferno.Identity.Entities;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>Builds account links for <see cref="InfernoAccountController{TUser}"/>.</summary>
public static class UrlHelperExtensions
{
    /// <summary>Builds the absolute email-confirmation URL for <paramref name="userId"/>.</summary>
    /// <typeparam name="TUser">The user type the account controller is closed over.</typeparam>
    /// <param name="urlHelper">The URL helper for the current request.</param>
    /// <param name="userId">The user id placed in the query.</param>
    /// <param name="code">The confirmation code placed in the query.</param>
    /// <param name="scheme">The URL scheme, such as https.</param>
    /// <returns>The confirmation URL, or <see langword="null"/> when no matching route exists.</returns>
    public static string EmailConfirmationLink<TUser>(this IUrlHelper urlHelper, string userId, string code, string scheme)
        where TUser : InfernoIdentityUser, new() => urlHelper.Action(
            action: nameof(InfernoAccountController<TUser>.ConfirmEmail),
            controller: "Account",
            values: new { userId, code },
            protocol: scheme);

    /// <summary>Builds the absolute password-reset URL for <paramref name="userId"/>.</summary>
    /// <typeparam name="TUser">The user type the account controller is closed over.</typeparam>
    /// <param name="urlHelper">The URL helper for the current request.</param>
    /// <param name="userId">The user id placed in the query.</param>
    /// <param name="code">The reset code placed in the query.</param>
    /// <param name="scheme">The URL scheme, such as https.</param>
    /// <returns>The reset URL, or <see langword="null"/> when no matching route exists.</returns>
    public static string ResetPasswordCallbackLink<TUser>(this IUrlHelper urlHelper, string userId, string code, string scheme)
        where TUser : InfernoIdentityUser, new() => urlHelper.Action(
            action: nameof(InfernoAccountController<TUser>.ResetPassword),
            controller: "Account",
            values: new { userId, code },
            protocol: scheme);
}