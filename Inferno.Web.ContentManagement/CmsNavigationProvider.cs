using Dependo;
using Inferno.Web.Navigation;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.ContentManagement
{
    public class CmsNavigationProvider : INavigationProvider
    {
        public CmsNavigationProvider()
        {
            T = EngineContext.Current.Resolve<IStringLocalizer>();
        }

        public IStringLocalizer T { get; set; }

        #region INavigationProvider Members

        public string MenuName => InfernoWebConstants.Areas.Admin;

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T[InfernoCmsLocalizableStrings.Navigation.CMS].Value, "2", BuildCmsMenu);
        }

        #endregion INavigationProvider Members

        private void BuildCmsMenu(NavigationItemBuilder builder)
        {
            builder.Icon("fa fa-edit");

            // Blog
            builder.Add(T[InfernoCmsLocalizableStrings.Blog.Title].Value, "5", item => item
                .Url("/admin/blog/index")
                .Icon("fa fa-bullhorn")
                .Permission(CmsConstants.Policies.BlogRead));

            // Content Blocks
            builder.Add(T[InfernoCmsLocalizableStrings.ContentBlocks.Title].Value, "5", item => item
                .Url("/admin/content-blocks/index")
                .Icon("fa fa-th-large")
                .Permission(CmsConstants.Policies.ContentBlocksRead));

            // Media
            builder.Add(T[InfernoCmsLocalizableStrings.Media.Title].Value, "5", item => item
                .Url("/admin/media/index")
                .Icon("fa fa-picture-o")
                .Permission(CmsConstants.Policies.MediaRead));

            // Menus
            builder.Add(T[InfernoCmsLocalizableStrings.Menus.Title].Value, "5", item => item
                .Url("/admin/menus/index")
                .Icon("fa fa-arrow-right")
                .Permission(CmsConstants.Policies.MenusRead));

            // Pages
            builder.Add(T[InfernoCmsLocalizableStrings.Pages.Title].Value, "5", item => item
                .Url("/admin/pages/index")
                .Icon("fa fa-file-o")
                .Permission(CmsConstants.Policies.PagesRead));

            //// Subscribers
            //builder.Add(T[InfernoCmsLocalizableStrings.Newsletters.Subscribers].Value, "5", item => item
            //    .Url("/admin/newsletters/subscribers")
            //    .Icon("fa fa-users")
            //    .Permission(CmsConstants.Policies.NewsletterRead));

            // XML Sitemap
            builder.Add(T[InfernoCmsLocalizableStrings.Sitemap.XMLSitemap].Value, "5", item => item
                .Url("/admin/sitemap/xml-sitemap")
                .Icon("fa fa-sitemap")
                .Permission(CmsConstants.Policies.SitemapRead));
        }
    }
}