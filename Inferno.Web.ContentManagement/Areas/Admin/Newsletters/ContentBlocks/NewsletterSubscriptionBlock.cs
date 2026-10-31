using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.ContentBlocks;

public class NewsletterSubscriptionBlock : ContentBlockBase
{
    public override string Name => "Newsletter Subscription Block";

    public override Type EditorType => typeof(Components.NewsletterSubscriptionBlockEditor);

    public override Type DisplayType => typeof(Components.NewsletterSubscriptionBlockDisplay);
}
