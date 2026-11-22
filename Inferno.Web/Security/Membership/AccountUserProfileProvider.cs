using Dependo;
using Localize;
using Inferno.Security.Membership;

namespace Inferno.Web.Security.Membership;

/// <summary>
/// The account name fields: family name, given names, and whether the family name is shown first.
/// The show-family-name flag is parsed as a Boolean.
/// </summary>
public class AccountUserProfileProvider : IUserProfileProvider
{
    /// <summary>Profile keys for <see cref="AccountUserProfileProvider"/>.</summary>
    public class Fields
    {
        /// <summary>The family name.</summary>
        public const string FamilyName = "FamilyName";

        /// <summary>The given names.</summary>
        public const string GivenNames = "GivenNames";

        /// <summary>Whether the family name is shown before the given names.</summary>
        public const string ShowFamilyNameFirst = "ShowFamilyNameFirst";
    }

    /// <summary>The family name loaded for the user.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.UserProfile.Account.FamilyName)]
    public string FamilyName { get; set; }

    /// <summary>The given names loaded for the user.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.UserProfile.Account.GivenNames)]
    public string GivenNames { get; set; }

    /// <summary>Whether the family name is shown before the given names.</summary>
    [LocalizedDisplayName(InfernoWebLocalizableStrings.UserProfile.Account.ShowFamilyNameFirst)]
    public bool ShowFamilyNameFirst { get; set; }

    #region IUserProfileProvider Members

    /// <inheritdoc />
    public string Name => "Account";

    /// <inheritdoc />
    public string DisplayTemplatePath => "Inferno.Web.Views.Shared.DisplayTemplates.AccountUserProfileProvider";

    /// <inheritdoc />
    public string EditorTemplatePath => typeof(Components.AccountUserProfileEditor).AssemblyQualifiedName;

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public IEnumerable<string> GetFieldNames() => new[]
        {
            Fields.FamilyName,
            Fields.GivenNames,
            Fields.ShowFamilyNameFirst
        };

    /// <inheritdoc />
    public async Task PopulateFieldsAsync(string userId)
    {
        var membershipService = DependoResolver.Instance.Resolve<IMembershipService>();

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