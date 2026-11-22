using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.ContentBlocks;

/// <summary>A form that subscribes a visitor to the current tenant's newsletter.</summary>
public class NewsletterSubscriptionBlock : ContentBlockBase
{
    /// <inheritdoc />
    public override string Name => "Newsletter Subscription Block";

    /// <inheritdoc />
    public override Type EditorType => typeof(Components.NewsletterSubscriptionBlockEditor);

    /// <inheritdoc />
    public override Type DisplayType => typeof(Components.NewsletterSubscriptionBlockDisplay);
}
