using Inferno.Web.Security.Permissions;
using Policies = Inferno.Web.ContentManagement.CmsConstants.Policies;

namespace Inferno.Web.ContentManagement.Security;

public sealed class CmsPermissionProvider : IPermissionProvider
{
    private const string Content = "Content";
    private const string Marketing = "Marketing";

    public IEnumerable<Permission> GetPermissions() =>
    [
        new(Policies.PagesRead, Content, "View pages"),
        new(Policies.PagesWrite, Content, "Edit pages") { Implies = [Policies.PagesRead] },
        new(Policies.PageHistoryRead, Content, "View page history"),
        new(Policies.PageHistoryRestore, Content, "Restore page versions") { Implies = [Policies.PageHistoryRead] },
        new(Policies.PageHistoryWrite, Content, "Delete page history") { Implies = [Policies.PageHistoryRead] },
        new(Policies.ContentBlocksRead, Content, "View content blocks"),
        new(Policies.ContentBlocksWrite, Content, "Edit content blocks") { Implies = [Policies.ContentBlocksRead] },
        new(Policies.ContentZonesRead, Content, "View content zones"),
        new(Policies.ContentZonesWrite, Content, "Edit content zones") { Implies = [Policies.ContentZonesRead] },
        new(Policies.MenusRead, Content, "View menus"),
        new(Policies.MenusWrite, Content, "Edit menus") { Implies = [Policies.MenusRead] },
        new(Policies.MediaRead, Content, "View media"),
        new(Policies.MediaWrite, Content, "Upload and delete media") { Implies = [Policies.MediaRead] },
        new(Policies.BlogRead, Content, "View the blog"),
        new(Policies.BlogWrite, Content, "Write blog posts") { Implies = [Policies.BlogRead] },

        new(Policies.NewsletterRead, Marketing, "View newsletter subscribers"),
        new(Policies.NewsletterWrite, Marketing, "Manage newsletter subscribers") { Implies = [Policies.NewsletterRead] },
        new(Policies.MessagingRead, Marketing, "View message templates and queued email"),
        new(Policies.MessagingWrite, Marketing, "Edit message templates and queued email") { Implies = [Policies.MessagingRead] },
        new(Policies.SitemapRead, Marketing, "View the XML sitemap"),
        new(Policies.SitemapWrite, Marketing, "Edit the XML sitemap") { Implies = [Policies.SitemapRead] },
    ];
}
