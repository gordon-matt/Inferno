using Dependo;
using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Controllers.Api
{
    public class PageTreeApiController : ODataController
    {
        private readonly IRepository<Page> repository;
        private readonly IWorkContext workContext;

        public PageTreeApiController(IRepository<Page> repository, IWorkContext workContext)
        {
            this.repository = repository;
            this.workContext = workContext;
        }

        public async Task<IEnumerable<PageTreeItem>> Get(ODataQueryOptions<PageTreeItem> options)
        {
            if (!await AuthorizeAsync(CmsConstants.Policies.PagesRead))
            {
                return Enumerable.Empty<PageTreeItem>();
            }

            int tenantId = GetTenantId();
            var pages = await repository.FindAsync(x => x.TenantId == tenantId);

            var hierarchy = pages
                .Where(x => x.ParentId == null)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name)
                .Select(x => new PageTreeItem
                {
                    Id = x.Id,
                    Title = x.Name,
                    IsEnabled = x.IsEnabled,
                    SubPages = GetSubPages(pages, x.Id).ToList()
                });

            var settings = new ODataValidationSettings
            {
                AllowedQueryOptions = AllowedQueryOptions.All,
                MaxExpansionDepth = 10
            };
            options.Validate(settings);

            var results = options.ApplyTo(hierarchy.AsQueryable());
            return (results as IQueryable<PageTreeItem>).ToHashSet();
        }

        [EnableQuery]
        public virtual async Task<SingleResult<PageTreeItem>> Get([FromODataUri] Guid key)
        {
            if (!await AuthorizeAsync(CmsConstants.Policies.PagesRead))
            {
                return SingleResult.Create(Enumerable.Empty<PageTreeItem>().AsQueryable());
            }

            int tenantId = GetTenantId();
            var pages = await repository.FindAsync(x => x.TenantId == tenantId);
            var entity = pages.FirstOrDefault(x => x.Id == key);

            return SingleResult.Create(new[] { entity }.Select(x => new PageTreeItem
            {
                Id = x.Id,
                Title = x.Name,
                IsEnabled = x.IsEnabled,
                SubPages = GetSubPages(pages, x.Id).ToList()
            }).AsQueryable());
        }

        private static IEnumerable<PageTreeItem> GetSubPages(IEnumerable<Page> pages, Guid parentId)
        {
            return pages
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Name)
                .Select(x => new PageTreeItem
                {
                    Id = x.Id,
                    Title = x.Name,
                    IsEnabled = x.IsEnabled,
                    SubPages = GetSubPages(pages, x.Id).ToList()
                });
        }

        protected virtual async Task<bool> AuthorizeAsync(string policyName)
        {
            var authorizationService = EngineContext.Current.Resolve<IAuthorizationService>();
            if (authorizationService == null || string.IsNullOrEmpty(policyName))
            {
                return true;
            }

            return (await authorizationService.AuthorizeAsync(User, policyName)).Succeeded;
        }

        protected virtual int GetTenantId() => workContext.CurrentTenant.Id;
    }

    public class PageTreeItem
    {
        public PageTreeItem()
        {
            SubPages = new List<PageTreeItem>();
        }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public bool IsEnabled { get; set; }

        public List<PageTreeItem> SubPages { get; set; }
    }
}