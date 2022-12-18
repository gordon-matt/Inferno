using Dependo;
using Inferno.Localization.ComponentModel;
using Inferno.Security.Membership;

namespace Inferno.Web.Security.Membership
{
    public class AccountUserProfileProvider : IUserProfileProvider
    {
        public class Fields
        {
            public const string FamilyName = "FamilyName";
            public const string GivenNames = "GivenNames";
            public const string ShowFamilyNameFirst = "ShowFamilyNameFirst";
        }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.UserProfile.Account.FamilyName)]
        public string FamilyName { get; set; }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.UserProfile.Account.GivenNames)]
        public string GivenNames { get; set; }

        [LocalizedDisplayName(InfernoWebLocalizableStrings.UserProfile.Account.ShowFamilyNameFirst)]
        public bool ShowFamilyNameFirst { get; set; }

        #region IUserProfileProvider Members

        public string Name => "Account";

        public string DisplayTemplatePath => "Inferno.Web.Views.Shared.DisplayTemplates.AccountUserProfileProvider";

        public string EditorTemplatePath => "Inferno.Web.Views.Shared.EditorTemplates.AccountUserProfileProvider";

        public int Order => 0;

        public IEnumerable<string> GetFieldNames()
        {
            return new[]
            {
                Fields.FamilyName,
                Fields.GivenNames,
                Fields.ShowFamilyNameFirst
            };
        }

        public async Task PopulateFieldsAsync(string userId)
        {
            var membershipService = EngineContext.Current.Resolve<IMembershipService>();

            var profile = await membershipService.GetProfileAsync(userId);

            if (profile.ContainsKey(Fields.FamilyName))
            {
                FamilyName = profile[Fields.FamilyName];
            }
            if (profile.ContainsKey(Fields.GivenNames))
            {
                GivenNames = profile[Fields.GivenNames];
            }
            if (profile.ContainsKey(Fields.ShowFamilyNameFirst))
            {
                ShowFamilyNameFirst = bool.Parse(profile[Fields.ShowFamilyNameFirst]);
            }
        }

        #endregion IUserProfileProvider Members
    }
}