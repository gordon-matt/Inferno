using Dependo;
using Inferno.Web.Navigation;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.ContentManagement;

public class CmsNavigationProvider : INavigationProvider
{
    public CmsNavigationProvider()
    {
        T = DependoResolver.Instance.Resolve<IStringLocalizer>();
    }

    public IStringLocalizer T { get; set; }

    #region INavigationProvider Members

    public string MenuName => InfernoWebConstants.Areas.Admin;

    public void GetNavigation(NavigationBuilder builder)
    {
        builder.Add(T[InfernoCmsLocalizableStrings.Navigation.CMS].Value, "2", BuildCmsMenu);
        builder.Add(T[InfernoCmsLocalizableStrings.Messaging.Title].Value, "2", BuildMessagingMenu);
    }

    #endregion INavigationProvider Members

    private void BuildCmsMenu(NavigationItemBuilder builder)
    {
        builder.Icon("fas fa-edit");

        // Blog
        builder.Add(T[InfernoCmsLocalizableStrings.Blog.Title].Value, "5", blog =>
        {
            blog.Icon("fas fa-paperclip").Permission(CmsConstants.Policies.BlogWrite);

            blog.Add(T[InfernoCmsLocalizableStrings.Blog.Posts].Value, "1", item => item
                .Url("/admin/blog/posts")
                .Icon("fas fa-file")
                .Permission(CmsConstants.Policies.BlogWrite));

            blog.Add(T[InfernoCmsLocalizableStrings.Blog.Categories].Value, "2", item => item
                .Url("/admin/blog/categories")
                .Icon("fas fa-folder")
                .Permission(CmsConstants.Policies.BlogWrite));

            blog.Add(T[InfernoCmsLocalizableStrings.Blog.Tags].Value, "3", item => item
                .Url("/admin/blog/tags")
                .Icon("fas fa-tag")
                .Permission(CmsConstants.Policies.BlogWrite));
        });

        // Content Blocks
        builder.Add(T[InfernoCmsLocalizableStrings.ContentBlocks.Title].Value, "5", item => item
            .Url("/admin/blocks/content-blocks")
            .Icon("fas fa-square")
            .Permission(CmsConstants.Policies.ContentBlocksRead));

        builder.Add(T[InfernoCmsLocalizableStrings.ContentBlocks.EntityTypeContentBlocks].Value, "5", item => item
            .Url("/admin/blocks/entity-type-content-blocks")
            .Icon("fas fa-th-large")
            .Permission(CmsConstants.Policies.ContentBlocksRead));

        // Media
        builder.Add(T[InfernoCmsLocalizableStrings.Media.Title].Value, "5", item => item
            .Url("/admin/media/index")
            .Icon("fas fa-image")
            .Permission(CmsConstants.Policies.MediaRead));

        // Menus
        builder.Add(T[InfernoCmsLocalizableStrings.Menus.Title].Value, "5", item => item
            .Url("/admin/menus/index")
            .Icon("fas fa-arrow-right")
            .Permission(CmsConstants.Policies.MenusRead));

        // Pages
        builder.Add(T[InfernoCmsLocalizableStrings.Pages.Title].Value, "5", item => item
            .Url("/admin/pages/index")
            .Icon("fas fa-code")
            .Permission(CmsConstants.Policies.PagesRead));

        // Subscribers
        builder.Add(T[InfernoCmsLocalizableStrings.Newsletters.Subscribers].Value, "5", item => item
            .Url("/admin/newsletters/subscribers")
            .Icon("fas fa-users")
            .Permission(CmsConstants.Policies.NewsletterRead));

        // XML Sitemap
        builder.Add(T[InfernoCmsLocalizableStrings.Sitemap.XMLSitemap].Value, "5", item => item
            .Url("/admin/sitemap/xml-sitemap")
            .Icon("fas fa-map")
            .Permission(CmsConstants.Policies.SitemapRead));
    }

    private void BuildMessagingMenu(NavigationItemBuilder builder)
    {
        builder.Icon("fas fa-envelope");

        builder.Add(T[InfernoCmsLocalizableStrings.Messaging.MessageTemplates].Value, "5", item => item
            .Url("/admin/messaging/templates")
            .Icon("fas fa-envelope")
            .Permission(CmsConstants.Policies.MessagingRead));

        builder.Add(T[InfernoCmsLocalizableStrings.Messaging.QueuedEmails].Value, "5", item => item
            .Url("/admin/messaging/queued-emails")
            .Icon("fas fa-inbox")
            .Permission(CmsConstants.Policies.MessagingRead));
    }
}