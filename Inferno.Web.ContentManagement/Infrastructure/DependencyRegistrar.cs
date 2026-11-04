using Dependo;
using Inferno.Web.ContentManagement.Localization;
using Localize;
using Inferno.Web.Configuration;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Localization;
using Inferno.Web.ContentManagement.Areas.Admin.Media.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Models;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging;
using Inferno.Web.Infrastructure;
using Inferno.Web.Messaging;
using Inferno.Web.Navigation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.ContentManagement.Infrastructure;

public class DependencyRegistrar : IDependencyRegistrar
{
    #region IDependencyRegistrar Members

    public void Register(IContainerBuilder builder, ITypeFinder typeFinder, IConfiguration configuration)
    {
        builder.Register<IRouterAssemblyMarker, RouterAssemblyMarker>(ServiceLifetime.Singleton);

        // Embedded File Provider
        //builder.Register<IEmbeddedFileProviderRegistrar, EmbeddedFileProviderRegistrar>(ServiceLifetime.Scoped);

        #region Services

        // Menus
        builder.Register<IMenuService, MenuService>(ServiceLifetime.Transient);
        builder.Register<IMenuItemService, MenuItemService>(ServiceLifetime.Transient);
        builder.Register<IMenuAdminService, MenuAdminService>(ServiceLifetime.Transient);

        // Pages
        builder.Register<IPageService, PageService>(ServiceLifetime.Transient);
        builder.Register<IPageVersionService, PageVersionService>(ServiceLifetime.Transient);
        builder.Register<IPageContentService, PageContentService>(ServiceLifetime.Transient);
        builder.Register<IPageAuthoringService, PageAuthoringService>(ServiceLifetime.Scoped);

        // Content Blocks
        builder.Register<IContentBlockService, ContentBlockService>(ServiceLifetime.Transient);
        builder.Register<IZoneService, ZoneService>(ServiceLifetime.Transient);
        builder.Register<IContentBlockAdminService, ContentBlockAdminService>(ServiceLifetime.Transient);

        // Sitemap
        builder.Register<ISitemapAdminService, SitemapAdminService>(ServiceLifetime.Transient);

        #endregion Services

        #region Localization

        builder.Register<ILanguagePack, LanguagePackInvariant>(ServiceLifetime.Singleton);
        builder.RegisterSelf<ContentLocalization>(ServiceLifetime.Scoped);

        #endregion Localization

        #region Navigation

        builder.Register<INavigationProvider, CmsNavigationProvider>(ServiceLifetime.Singleton);

        #endregion Navigation

        #region Security

        // User Profile Providers
        builder.Register<Inferno.Web.Security.Membership.IUserProfileProvider, NewsletterUserProfileProvider>(ServiceLifetime.Singleton);

        #endregion Security

        #region Configuration

        builder.Register<ISettings, PageSettings>(ServiceLifetime.Scoped);

        #endregion Configuration

        #region Content Blocks

        // Blogs
        builder.Register<IContentBlock, FilteredPostsBlock>(ServiceLifetime.Transient);
        builder.Register<IContentBlock, LastNPostsBlock>(ServiceLifetime.Transient);
        builder.Register<IContentBlock, TagCloudBlock>(ServiceLifetime.Transient);
        builder.Register<IContentBlock, CategoriesBlock>(ServiceLifetime.Transient);

        // Other
        builder.Register<IContentBlock, FormBlock>(ServiceLifetime.Transient);
        builder.Register<IContentBlock, HtmlBlock>(ServiceLifetime.Transient);
        builder.Register<IContentBlock, LanguageSwitchBlock>(ServiceLifetime.Transient);
        builder.Register<IContentBlock, Inferno.Web.ContentManagement.Areas.Admin.Newsletters.ContentBlocks.NewsletterSubscriptionBlock>(ServiceLifetime.Transient);
        builder.Register<IContentBlock, VideoBlock>(ServiceLifetime.Transient);

        #endregion Content Blocks

        #region Other: Content Blocks

        builder.Register<IContentBlockProvider, DefaultContentBlockProvider>(ServiceLifetime.Transient);

        builder.Register<Components.CmsMenuBuilder, Components.CmsMenuBuilder>(ServiceLifetime.Transient);
        builder.Register<Components.CmsBreadcrumbBuilder, Components.CmsBreadcrumbBuilder>(ServiceLifetime.Transient);
        builder.Register<Areas.Admin.Newsletters.Services.INewsletterService, Areas.Admin.Newsletters.Services.NewsletterService>(ServiceLifetime.Transient);
        builder.Register<ISubscriberAdminService, SubscriberAdminService>(ServiceLifetime.Transient);

        // Messaging
        builder.Register<IMessageTemplateAdminService, MessageTemplateAdminService>(ServiceLifetime.Transient);
        builder.Register<IQueuedEmailAdminService, QueuedEmailAdminService>(ServiceLifetime.Transient);
        builder.Register<Events.IConsumer<Areas.Admin.Newsletters.Events.NewsletterSubscribedEvent>, Areas.Admin.Newsletters.NewsletterEmailConsumer>(ServiceLifetime.Transient);
        builder.Register<Events.IConsumer<Areas.Admin.Newsletters.Events.NewsletterUnsubscribedEvent>, Areas.Admin.Newsletters.NewsletterEmailConsumer>(ServiceLifetime.Transient);
        builder.Register<Areas.Admin.ContentBlocks.Services.IFormBlockSubmissionService, Areas.Admin.ContentBlocks.Services.FormBlockSubmissionService>(ServiceLifetime.Transient);

        #endregion Other: Content Blocks


        // Indexing
        //builder.Register<IIndexingContentProvider, BlogIndexingContentProvider>(ServiceLifetime.Transient); // TODO

        builder.Register<IMessageTemplateRenderer, PageKrafterMessageTemplateRenderer>(ServiceLifetime.Transient);
    }

    public int Order => 1;

    #endregion IDependencyRegistrar Members
}
