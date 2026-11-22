using Dependo;
using Localize;
using Inferno.Security.Membership;
using Inferno.Web.Security.Membership;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters;

/// <summary>Stores whether a user is subscribed to the newsletter, as a profile entry.</summary>
public class NewsletterUserProfileProvider : IUserProfileProvider
{
    /// <summary>Profile keys written by this provider.</summary>
    public static class Fields
    {
        /// <summary>The profile key whose value is <see langword="true"/> or <see langword="false"/>.</summary>
        public const string SubscribeToNewsletters = "SubscribeToNewsletters";
    }

    /// <summary>Whether the user loaded by <see cref="PopulateFieldsAsync"/> is subscribed.</summary>
    [LocalizedDisplayName(InfernoCmsLocalizableStrings.UserProfile.Newsletter.SubscribeToNewsletters)]
    public bool SubscribeToNewsletters { get; set; }

    #region IUserProfileProvider Members

    /// <summary>The profile section name.</summary>
    public string Name => "Newsletters";

    /// <summary>The display template path. The newsletter profile is edited by a component instead.</summary>
    public string DisplayTemplatePath => "Inferno.Web.ContentManagement.Views.Shared.DisplayTemplates.NewsletterUserProfileProvider";

    /// <summary>The assembly-qualified name of the newsletter profile editor component.</summary>
    public string EditorTemplatePath => typeof(Components.NewsletterUserProfileEditor).AssemblyQualifiedName;

    /// <summary>Sort position among profile providers. A high value lists this section late.</summary>
    public int Order => 9999;

    /// <summary>Returns <see cref="Fields.SubscribeToNewsletters"/>.</summary>
    /// <returns>The one profile key this provider owns.</returns>
    public IEnumerable<string> GetFieldNames() =>
    [
        Fields.SubscribeToNewsletters
    ];

    /// <summary>Loads <see cref="SubscribeToNewsletters"/> from the user's profile. A missing or unreadable value leaves it <see langword="false"/>.</summary>
    /// <param name="userId">The user whose profile is read.</param>
    public async Task PopulateFieldsAsync(string userId)
    {
        var membershipService = DependoResolver.Instance.Resolve<IMembershipService>();

        var profile = await membershipService.GetProfileAsync(userId);
        if (profile.TryGetValue(Fields.SubscribeToNewsletters, out string value) && bool.TryParse(value, out bool parsed))
        {
            SubscribeToNewsletters = parsed;
        }
    }

    #endregion IUserProfileProvider Members
}