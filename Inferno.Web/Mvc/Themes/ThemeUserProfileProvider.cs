using Dependo;
using Inferno.Localization.ComponentModel;
using Inferno.Security.Membership;
using Inferno.Threading;
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

        public string Name
        {
            get { return "Theme"; }
        }

        public string DisplayTemplatePath
        {
            get { return "Inferno.Web.Views.Shared.DisplayTemplates.ThemeUserProfileProvider"; }
        }

        public string EditorTemplatePath
        {
            get { return "Inferno.Web.Views.Shared.EditorTemplates.ThemeUserProfileProvider"; }
        }

        public int Order
        {
            get { return 9999; }
        }

        public IEnumerable<string> GetFieldNames()
        {
            return new[]
            {
                Fields.PreferredTheme
            };
        }

        public void PopulateFields(string userId)
        {
            var membershipService = EngineContext.Current.Resolve<IMembershipService>();
            PreferredTheme = AsyncHelper.RunSync(() => membershipService.GetProfileEntry(userId, Fields.PreferredTheme));
        }

        #endregion IUserProfileProvider Members
    }
}