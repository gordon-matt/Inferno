using Inferno.Identity;
using Inferno.Identity.Entities;

namespace Microsoft.AspNetCore.Mvc
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink<TUser>(this IUrlHelper urlHelper, string userId, string code, string scheme)
            where TUser : InfernoIdentityUser, new()
        {
            return urlHelper.Action(
                action: nameof(InfernoAccountController<TUser>.ConfirmEmail),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }

        public static string ResetPasswordCallbackLink<TUser>(this IUrlHelper urlHelper, string userId, string code, string scheme)
            where TUser : InfernoIdentityUser, new()
        {
            return urlHelper.Action(
                action: nameof(InfernoAccountController<TUser>.ResetPassword),
                controller: "Account",
                values: new { userId, code },
                protocol: scheme);
        }
    }
}