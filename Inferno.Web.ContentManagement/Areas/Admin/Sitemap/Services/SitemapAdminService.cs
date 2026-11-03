using System.Text;
using System.Xml.Serialization;
using Ardalis.Result;
using Extenso;
using Extenso.Data.Entity;
using Inferno.Tenants;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Models;
using Localize.Languages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Services;

public interface ISitemapAdminService
{
    /// <summary>
    /// One entry per current page. Entries are added for new pages and removed for deleted ones first,
    /// so the list always matches the site.
    /// </summary>
    Task<Result<IReadOnlyList<SitemapConfigModel>>> ListAsync(int tenantId);

    Task<Result> UpdateAsync(int tenantId, int id, ChangeFrequency changeFrequency, float priority);

    /// <summary>Writes the tenant's sitemap.xml, using <paramref name="requestBaseUrl"/> when the tenant has no public URL of its own.</summary>
    Task<Result> GenerateAsync(int tenantId, string requestBaseUrl);

    /// <summary>Where the tenant's generated sitemap is stored. Always absolute, because the file writer cannot create a file in the current directory from a bare file name.</summary>
    string GetFilePath(int tenantId);
}

public class SitemapAdminService(
    IRepository<SitemapConfig> repository,
    IPageVersionService pageVersionService,
    ILanguageService languageService,
    IWorkContext workContext,
    IHostEnvironment hostEnvironment,
    ILogger<SitemapAdminService> logger) : ISitemapAdminService
{
    private const float DEFAULT_PRIORITY = .5f;

    public async Task<Result<IReadOnlyList<SitemapConfigModel>>> ListAsync(int tenantId)
    {
        try
        {
            var pageVersions = pageVersionService.GetCurrentVersions(tenantId, shownOnMenusOnly: false).ToList();
            var config = await SyncAsync(tenantId, pageVersions);
            var slugs = pageVersions.ToDictionary(version => version.Id, version => version.Slug);

            IReadOnlyList<SitemapConfigModel> models = config
                .Where(item => slugs.ContainsKey(item.PageId))
                .Select(item => new SitemapConfigModel
                {
                    Id = item.Id,
                    Location = slugs[item.PageId],
                    ChangeFrequency = item.ChangeFrequency,
                    Priority = item.Priority
                })
                .OrderBy(model => model.Location)
                .ToList();

            return Result.Success(models);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not load the sitemap configuration for tenant {TenantId}", tenantId);
            return Result<IReadOnlyList<SitemapConfigModel>>.Error("The sitemap configuration could not be loaded.");
        }
    }

    public async Task<Result> UpdateAsync(int tenantId, int id, ChangeFrequency changeFrequency, float priority)
    {
        if (priority is < 0f or > 1f)
        {
            return Result.Invalid(new ValidationError { Identifier = nameof(SitemapConfig.Priority), ErrorMessage = "Priority must be between 0 and 1." });
        }

        var entity = await repository.FindOneAsync(id);
        if (entity is null || entity.TenantId != tenantId)
        {
            return Result.NotFound();
        }

        entity.ChangeFrequency = changeFrequency;
        entity.Priority = (float)Math.Round(priority, 1);
        await repository.UpdateAsync(entity);
        return Result.Success();
    }

    public async Task<Result> GenerateAsync(int tenantId, string requestBaseUrl)
    {
        try
        {
            var config = await repository.FindAsync(new SearchOptions<SitemapConfig> { Query = x => x.TenantId == tenantId });
            var pageVersions = (await pageVersionService.FindAsync(new SearchOptions<PageVersion> { Query = x => x.TenantId == tenantId })).ToList();

            var listed = await languageService.ListAsync();
            List<string> cultures = listed.IsSuccess
                ? listed.Value.Select(language => language.CultureCode).Where(culture => !string.IsNullOrEmpty(culture)).ToList()
                : [];

            string siteUrl = workContext.CurrentTenant.GetPublicBaseUrl(requestBaseUrl).TrimEnd('/');
            var urls = new List<UrlElement>();

            foreach (var item in config)
            {
                var invariantVersion = pageVersions.FirstOrDefault(x => x.CultureCode == null && x.Id == item.PageId);
                if (invariantVersion is null && cultures.Count == 1)
                {
                    // With a single language configured, that language's version stands in for the invariant one.
                    invariantVersion = pageVersions.FirstOrDefault(x => x.CultureCode == cultures[0] && x.Id == item.PageId);
                }

                if (invariantVersion is null)
                {
                    continue;
                }

                if (cultures.Count > 1)
                {
                    urls.AddRange(LocalizedUrls(item, invariantVersion, pageVersions, cultures, siteUrl));
                    continue;
                }

                urls.Add(new UrlElement
                {
                    Location = $"{siteUrl}/{invariantVersion.Slug}",
                    LastModified = invariantVersion.DateModifiedUtc.ToString("yyyy-MM-dd"),
                    ChangeFrequency = item.ChangeFrequency,
                    Priority = item.Priority,
                    Links = null
                });
            }

            var file = new SitemapXmlFile { Urls = urls.OrderBy(x => x.Location).ToHashSet() };
            var xmlns = new XmlSerializerNamespaces();
            xmlns.Add("xhtml", "http://www.w3.org/1999/xhtml");

            bool written = file.XmlSerialize(
                GetFilePath(tenantId),
                omitXmlDeclaration: false,
                xmlns: xmlns,
                encoding: Encoding.UTF8);

            if (!written)
            {
                logger.LogError("Could not write the sitemap file {Path} for tenant {TenantId}", GetFilePath(tenantId), tenantId);
                return Result.Error("The sitemap file could not be written.");
            }

            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not generate the sitemap for tenant {TenantId}", tenantId);
            return Result.Error("The sitemap file could not be generated.");
        }
    }

    public string GetFilePath(int tenantId) => Path.Combine(hostEnvironment.ContentRootPath, $"sitemap-{tenantId}.xml");

    private async Task<IReadOnlyList<SitemapConfig>> SyncAsync(int tenantId, IReadOnlyList<PageVersion> pageVersions)
    {
        var config = await repository.FindAsync(new SearchOptions<SitemapConfig> { Query = x => x.TenantId == tenantId });

        var configPageIds = config.Select(x => x.PageId).ToHashSet();
        var currentIds = pageVersions.Select(x => x.Id).ToHashSet();

        var removedIds = configPageIds.Except(currentIds).ToList();
        var addedIds = currentIds.Except(configPageIds).ToHashSet();

        if (removedIds.Count == 0 && addedIds.Count == 0)
        {
            return config.ToList();
        }

        if (removedIds.Count > 0)
        {
            await repository.DeleteAsync(x => x.TenantId == tenantId && removedIds.Contains(x.PageId));
        }

        if (addedIds.Count > 0)
        {
            await repository.InsertAsync(pageVersions
                .Where(x => addedIds.Contains(x.Id))
                .Select(x => new SitemapConfig
                {
                    TenantId = tenantId,
                    PageId = x.Id,
                    ChangeFrequency = ChangeFrequency.Weekly,
                    Priority = DEFAULT_PRIORITY
                })
                .ToList());
        }

        return (await repository.FindAsync(new SearchOptions<SitemapConfig> { Query = x => x.TenantId == tenantId })).ToList();
    }

    // One URL per language, each listing the others as hreflang alternates.
    private static IEnumerable<UrlElement> LocalizedUrls(
        SitemapConfig item,
        PageVersion invariantVersion,
        IReadOnlyList<PageVersion> pageVersions,
        IReadOnlyList<string> cultures,
        string siteUrl)
    {
        var localizedVersions = pageVersions.Where(x => x.PageId == invariantVersion.PageId && x.CultureCode != null).ToList();

        PageVersion VersionFor(string culture) =>
            localizedVersions
                .Where(x => x.CultureCode == culture)
                .OrderByDescending(x => x.DateModifiedUtc)
                .FirstOrDefault() ?? invariantVersion;

        foreach (string culture in cultures)
        {
            var version = VersionFor(culture);
            var links = cultures
                .Select(other => new LinkElement
                {
                    Rel = "alternate",
                    HrefLang = other,
                    Href = $"{siteUrl}/{VersionFor(other).Slug}"
                })
                .OrderBy(x => x.HrefLang)
                .ToList();

            yield return new UrlElement
            {
                Location = $"{siteUrl}/{version.Slug}",
                LastModified = version.DateModifiedUtc.ToString("yyyy-MM-dd"),
                ChangeFrequency = item.ChangeFrequency,
                Priority = item.Priority,
                Links = links
            };
        }
    }
}
