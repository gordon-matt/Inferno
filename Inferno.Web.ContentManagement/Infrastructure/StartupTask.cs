using Dependo;
using Extenso.Collections;
using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.Pages;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.Infrastructure;

namespace Inferno.Web.ContentManagement.Infrastructure;

/// <summary>
/// Keeps the <see cref="PageType"/> table in sync with the <see cref="InfernoPageType"/>-derived
/// classes discovered at runtime. Without this, the Pages admin UI cannot offer a page type
/// to choose from when creating a new page.
/// </summary>
public class StartupTask : IStartupTask
{
    public int Order => 100;

    public Task ExecuteAsync()
    {
        EnsurePageTypes();
        return Task.CompletedTask;
    }

    private static void EnsurePageTypes()
    {
        var pageTypeService = DependoResolver.Instance.Resolve<IPageTypeService>();
        var pageTypeRepository = DependoResolver.Instance.Resolve<IRepository<PageType>>();

        var allPageTypes = pageTypeService.GetInfernoPageTypes().ToList();
        var allPageTypeNames = allPageTypes.Select(x => x.Name).ToList();

        var installedPageTypes = pageTypeRepository.Find(new SearchOptions<PageType>
        {
            Query = x => true
        }).ToList();
        var installedPageTypeNames = installedPageTypes.Select(x => x.Name).ToList();

        var pageTypesToAdd = allPageTypes
            .Where(x => !installedPageTypeNames.Contains(x.Name))
            .Select(x => new PageType
            {
                Id = Guid.NewGuid(),
                Name = x.Name
            })
            .ToList();

        if (!pageTypesToAdd.IsNullOrEmpty())
        {
            pageTypeRepository.Insert(pageTypesToAdd);
        }

        var pageTypesToDeleteIds = installedPageTypes
            .Where(x => !allPageTypeNames.Contains(x.Name))
            .Select(x => x.Id)
            .ToList();

        if (!pageTypesToDeleteIds.IsNullOrEmpty())
        {
            pageTypeRepository.Delete(x => pageTypesToDeleteIds.Contains(x.Id));
        }
    }
}