using Dependo;
using Inferno.Localization.ComponentModel;
using Inferno.Security.Membership;
using Inferno.Web.Security.Membership;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters;

public class NewsletterUserProfileProvider : IUserProfileProvider
{
    public static class Fields
    {
        public const string SubscribeToNewsletters = "SubscribeToNewsletters";
    }

    [LocalizedDisplayName(InfernoCmsLocalizableStrings.UserProfile.Newsletter.SubscribeToNewsletters)]
    public bool SubscribeToNewsletters { get; set; }

    #region IUserProfileProvider Members

    public string Name => "Newsletters";

    public string DisplayTemplatePath => "Inferno.Web.ContentManagement.Views.Shared.DisplayTemplates.NewsletterUserProfileProvider";

    public string EditorTemplatePath => "Inferno.Web.ContentManagement.Views.Shared.EditorTemplates.NewsletterUserProfileProvider";

    public int Order => 9999;

    public IEnumerable<string> GetFieldNames() =>
    [
        Fields.SubscribeToNewsletters
    ];

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