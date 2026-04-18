using Inferno.Web.Security;
using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.ContentManagement.Security
{
    /// <summary>
    /// Registers authorization policies for the CMS module. Each policy uses the same
    /// "Administrators role bypass or specific Permission claim" semantics as the rest
    /// of Inferno's policies — see <see cref="AuthorizationOptionsExtensions.AddPermission"/>.
    /// </summary>
    public static class CmsAuthorizationOptionsExtensions
    {
        public static void AddInfernoCmsPolicies(this AuthorizationOptions options)
        {
            options.AddPermission(CmsConstants.Policies.BlogRead, "BlogRead");
            options.AddPermission(CmsConstants.Policies.BlogWrite, "BlogWrite");

            options.AddPermission(CmsConstants.Policies.ContentBlocksRead, "ContentBlocksRead");
            options.AddPermission(CmsConstants.Policies.ContentBlocksWrite, "ContentBlocksWrite");
            options.AddPermission(CmsConstants.Policies.ContentZonesRead, "ContentZonesRead");
            options.AddPermission(CmsConstants.Policies.ContentZonesWrite, "ContentZonesWrite");

            options.AddPermission(CmsConstants.Policies.MediaRead, "MediaRead");
            options.AddPermission(CmsConstants.Policies.MediaWrite, "MediaWrite");

            options.AddPermission(CmsConstants.Policies.MenusRead, "MenusRead");
            options.AddPermission(CmsConstants.Policies.MenusWrite, "MenusWrite");

            options.AddPermission(CmsConstants.Policies.PageHistoryRead, "PageHistoryRead");
            options.AddPermission(CmsConstants.Policies.PageHistoryRestore, "PageHistoryRestore");
            options.AddPermission(CmsConstants.Policies.PageHistoryWrite, "PageHistoryWrite");
            options.AddPermission(CmsConstants.Policies.PagesRead, "PagesRead");
            options.AddPermission(CmsConstants.Policies.PagesWrite, "PagesWrite");
            options.AddPermission(CmsConstants.Policies.PageTypesRead, "PageTypesRead");
            options.AddPermission(CmsConstants.Policies.PageTypesWrite, "PageTypesWrite");

            options.AddPermission(CmsConstants.Policies.SitemapRead, "SitemapRead");
            options.AddPermission(CmsConstants.Policies.SitemapWrite, "SitemapWrite");
        }
    }
}
