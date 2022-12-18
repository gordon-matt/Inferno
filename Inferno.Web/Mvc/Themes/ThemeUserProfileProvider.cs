using Dependo;
using Inferno.Localization.ComponentModel;
using Inferno.Security.Membership;
using Inferno.Web.Security.Membership;

namespace Inferno.Web.Mvc.Themes
{
    public class ThemeUserProfileProvider : IUserProfileProvider
    {
        public class Fields
        {
            public const string PreferredTheme = "PreferredTheme";
        }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.UserProfile.Theme.PreferredTheme)]
        public string PreferredTheme { get; set; }

        #region IUserProfileProvider Members

        public string Name => "Theme";

        public string DisplayTemplatePath => "Inferno.Web.Views.Shared.DisplayTemplates.ThemeUserProfileProvider";

        public string EditorTemplatePath => "Inferno.Web.Views.Shared.EditorTemplates.ThemeUserProfileProvider";

        public int Order => 9999;

        public IEnumerable<string> GetFieldNames()
        {
            return new[]
            {
                Fields.PreferredTheme
            };
        }

        public async Task PopulateFieldsAsync(string userId)
        {
            var membershipService = EngineContext.Current.Resolve<IMembershipService>();
            PreferredTheme = await membershipService.GetProfileEntryAsync(userId, Fields.PreferredTheme);
        }

        #endregion IUserProfileProvider Members
    }
}