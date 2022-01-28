using Inferno.Localization;

namespace Inferno.Identity.Infrastructure
{
    public class LanguagePackInvariant : ILanguagePack
    {
        #region ILanguagePack Members

        public string CultureCode => null;

        public IDictionary<string, string> LocalizedStrings => new Dictionary<string, string>
        {
            { InfernoIdentityLocalizableStrings.ConfirmNewPassword, "Confirm New Password" },
            { InfernoIdentityLocalizableStrings.ConfirmPassword, "Confirm Password" },
            { InfernoIdentityLocalizableStrings.EditMyProfile, "Edit My Profile" },
            { InfernoIdentityLocalizableStrings.EditProfile, "Edit Profile" },
            { InfernoIdentityLocalizableStrings.Email, "Email" },
            { InfernoIdentityLocalizableStrings.InvalidUserNameOrPassword, "Invalid username or password." },
            { InfernoIdentityLocalizableStrings.Login, "Log in" },
            { InfernoIdentityLocalizableStrings.LogOut, "Log out" },
            { InfernoIdentityLocalizableStrings.ManageMessages.ChangePasswordSuccess, "Your password has been changed." },
            { InfernoIdentityLocalizableStrings.ManageMessages.Error, "An error has occurred." },
            { InfernoIdentityLocalizableStrings.ManageMessages.RemoveLoginSuccess, "The external login was removed." },
            { InfernoIdentityLocalizableStrings.ManageMessages.SetPasswordSuccess, "Your password has been set." },
            { InfernoIdentityLocalizableStrings.MyProfile, "My Profile" },
            { InfernoIdentityLocalizableStrings.NewPassword, "New Password" },
            { InfernoIdentityLocalizableStrings.NoUserFound, "No user found." },
            { InfernoIdentityLocalizableStrings.OldPassword, "Current Password" },
            { InfernoIdentityLocalizableStrings.Password, "Password" },
            { InfernoIdentityLocalizableStrings.ProfileForUser, "Profile For '{0}'" },
            { InfernoIdentityLocalizableStrings.Register, "Register" },
            { InfernoIdentityLocalizableStrings.RememberMe, "Remember Me?" },
            { InfernoIdentityLocalizableStrings.Title, "Account" },
            { InfernoIdentityLocalizableStrings.UserDoesNotExistOrIsNotConfirmed, "The user either does not exist or is not confirmed." }
        };

        #endregion ILanguagePack Members
    }
}