using Extenso.AspNetCore.OData;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Controllers.Api;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;

namespace Inferno.Web.ContentManagement.Infrastructure
{
    public class ODataRegistrar : IODataRegistrar
    {
        #region IODataRegistrar Members

        public void Register(ODataOptions options)
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();

            // Blog
            builder.EntitySet<BlogCategory>(CmsConstants.ODataRoutes.EntitySetNames.BlogCategory);
            builder.EntitySet<BlogPost>(CmsConstants.ODataRoutes.EntitySetNames.BlogPost);
            builder.EntitySet<BlogPostTag>(CmsConstants.ODataRoutes.EntitySetNames.BlogPostTag).EntityType.HasKey(x => new { x.PostId, x.TagId });
            builder.EntitySet<BlogTag>(CmsConstants.ODataRoutes.EntitySetNames.BlogTag);

            // Content Blocks
            builder.EntitySet<ContentBlock>(CmsConstants.ODataRoutes.EntitySetNames.ContentBlock);
            builder.EntitySet<EntityTypeContentBlock>(CmsConstants.ODataRoutes.EntitySetNames.EntityTypeContentBlock);
            builder.EntitySet<Zone>(CmsConstants.ODataRoutes.EntitySetNames.Zone);

            // Menus
            builder.EntitySet<Menu>(CmsConstants.ODataRoutes.EntitySetNames.Menu);
            builder.EntitySet<MenuItem>(CmsConstants.ODataRoutes.EntitySetNames.MenuItem);

            // Pages
            builder.EntitySet<Page>(CmsConstants.ODataRoutes.EntitySetNames.Page);
            builder.EntitySet<PageType>(CmsConstants.ODataRoutes.EntitySetNames.PageType);
            builder.EntitySet<PageVersion>(CmsConstants.ODataRoutes.EntitySetNames.PageVersion);
            builder.EntitySet<PageTreeItem>(CmsConstants.ODataRoutes.EntitySetNames.PageTree);

            // Other
            builder.EntitySet<SitemapConfig>(CmsConstants.ODataRoutes.EntitySetNames.XmlSitemap);
            //builder.EntitySet<Subscriber>(CmsConstants.ODataRoutes.EntitySetNames.Subscriber);

            // Action Configurations
            RegisterContentBlockODataActions(builder);
            RegisterEntityTypeContentBlockODataActions(builder);
            RegisterPageODataActions(builder);
            RegisterPageVersionODataActions(builder);
            RegisterXmlSitemapODataActions(builder);

            options.AddRouteComponents($"odata/{CmsConstants.ODataRoutes.Prefix}", builder.GetEdmModel());
        }

        #endregion IODataRegistrar Members

        private static void RegisterContentBlockODataActions(ODataModelBuilder builder)
        {
            var getByPageIdFunction = builder.EntityType<ContentBlock>().Collection.Function("GetByPageId");
            getByPageIdFunction.Parameter<Guid>("pageId");
            getByPageIdFunction.ReturnsCollectionFromEntitySet<ContentBlock>(CmsConstants.ODataRoutes.EntitySetNames.ContentBlock);

            var getLocalizedActionFunction = builder.EntityType<ContentBlock>().Collection.Function("GetLocalized");
            getLocalizedActionFunction.Parameter<Guid>("id");
            getLocalizedActionFunction.Parameter<string>("cultureCode");
            getLocalizedActionFunction.ReturnsFromEntitySet<ContentBlock>(CmsConstants.ODataRoutes.EntitySetNames.ContentBlock);

            var saveLocalizedAction = builder.EntityType<ContentBlock>().Collection.Action("SaveLocalized");
            saveLocalizedAction.Parameter<string>("cultureCode");
            saveLocalizedAction.Parameter<ContentBlock>("entity");
            saveLocalizedAction.Returns<IActionResult>();
        }

        private static void RegisterEntityTypeContentBlockODataActions(ODataModelBuilder builder)
        {
            var getLocalizedActionFunction = builder.EntityType<EntityTypeContentBlock>().Collection.Function("GetLocalized");
            getLocalizedActionFunction.Parameter<Guid>("id");
            getLocalizedActionFunction.Parameter<string>("cultureCode");
            getLocalizedActionFunction.ReturnsFromEntitySet<EntityTypeContentBlock>(CmsConstants.ODataRoutes.EntitySetNames.EntityTypeContentBlock);

            var saveLocalizedAction = builder.EntityType<EntityTypeContentBlock>().Collection.Action("SaveLocalized");
            saveLocalizedAction.Parameter<string>("cultureCode");
            saveLocalizedAction.Parameter<EntityTypeContentBlock>("entity");
            saveLocalizedAction.Returns<IActionResult>();
        }

        private static void RegisterPageODataActions(ODataModelBuilder builder)
        {
            var getTopLevelPagesFunction = builder.EntityType<Page>().Collection.Function("GetTopLevelPages");
            getTopLevelPagesFunction.ReturnsFromEntitySet<Page>(CmsConstants.ODataRoutes.EntitySetNames.Page);
        }

        private static void RegisterPageVersionODataActions(ODataModelBuilder builder)
        {
            var restoreVersionAction = builder.EntityType<PageVersion>().Action("RestoreVersion");
            restoreVersionAction.Returns<IActionResult>();

            var getCurrentVersionFunction = builder.EntityType<PageVersion>().Collection.Function("GetCurrentVersion");
            getCurrentVersionFunction.Parameter<Guid>("pageId");
            getCurrentVersionFunction.Parameter<string>("cultureCode");
            getCurrentVersionFunction.Returns<EdmPageVersion>();
        }

        private static void RegisterXmlSitemapODataActions(ODataModelBuilder builder)
        {
            var getConfigFunction = builder.EntityType<SitemapConfig>().Collection.Function("GetConfig");
            getConfigFunction.ReturnsCollection<SitemapConfigModel>();

            var setConfigAction = builder.EntityType<SitemapConfig>().Collection.Action("SetConfig");
            setConfigAction.Parameter<int>("id");
            setConfigAction.Parameter<byte>("changeFrequency");
            setConfigAction.Parameter<float>("priority");
            setConfigAction.Returns<IActionResult>();

            var generateAction = builder.EntityType<SitemapConfig>().Collection.Action("Generate");
            generateAction.Returns<IActionResult>();
        }
    }
}