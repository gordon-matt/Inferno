using System.Linq.Expressions;
using Extenso;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Localization;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

public interface IContentBlockService : IGenericDataService<ContentBlock>
{
    IEnumerable<IContentBlock> GetContentBlocks(Guid pageId, string cultureCode);

    IEnumerable<IContentBlock> GetContentBlocks(string zoneName, string cultureCode, Guid? pageId = null, bool includeDisabled = false);

    /// <summary>Drops the cached block lists, for changes that do not go through this service, such as a translation.</summary>
    void ResetCache();
}

public class ContentBlockService : GenericDataService<ContentBlock>, IContentBlockService
{
    private readonly Lazy<IRepository<Zone>> zoneRepository;
    private readonly ContentLocalization localization;
    private readonly IWorkContext workContext;

    public ContentBlockService(
        ICacheManager cacheManager,
        IRepository<ContentBlock> repository,
        Lazy<IRepository<Zone>> zoneRepository,
        ContentLocalization localization,
        IWorkContext workContext)
        : base(cacheManager, repository)
    {
        this.zoneRepository = zoneRepository;
        this.localization = localization;
        this.workContext = workContext;
    }

    #region GenericDataService<ContentBlock> Overrides

    public override int Delete(Expression<Func<ContentBlock, bool>> filterExpression)
    {
        string entityType = typeof(ContentBlock).FullName;
        var ids = Find(new SearchOptions<ContentBlock> { Query = filterExpression }).Select(block => block.Id.ToString()).ToList();

        int rowsAffected = ids.Sum(id => localization.DeleteFor(entityType, id));
        rowsAffected += base.Delete(filterExpression);

        return rowsAffected;
    }

    public override int Delete(ContentBlock entity)
    {
        string entityType = typeof(ContentBlock).FullName;
        string entityId = entity.Id.ToString();

        int rowsAffected = localization.DeleteFor(entityType, entityId);
        rowsAffected += base.Delete(entity);

        return rowsAffected;
    }

    public void ResetCache() => ClearCache();

    protected override void ClearCache()
    {
        base.ClearCache();
        CacheManager.RemoveByPattern("Repository_ContentBlocks_ByPageId_.*");
        CacheManager.RemoveByPattern("Repository_ContentBlocks_ByPageIdAndZoneAndIncDisabled_.*");
    }

    #endregion GenericDataService<ContentBlock> Overrides

    #region IContentBlockService Members

    public IEnumerable<IContentBlock> GetContentBlocks(Guid pageId, string cultureCode)
    {
        string key = string.Format(
            "Repository_ContentBlocks_ByPageId_{0}_{1}",
            pageId,
            cultureCode);

        var records = CacheManager.Get(key, () => Find(new SearchOptions<ContentBlock>
        {
            Query = x => x.PageId == pageId
        }));

        return GetContentBlocks(records, cultureCode);
    }

    public IEnumerable<IContentBlock> GetContentBlocks(string zoneName, string cultureCode, Guid? pageId = null, bool includeDisabled = false)
    {
        int tenantId = GetTenantId();

        string key = string.Format(
            "Repository_ContentBlocks_ByPageIdAndZoneAndIncDisabled_{0}_{1}_{2}_{3}_{4}",
            tenantId,
            pageId,
            cultureCode,
            zoneName,
            includeDisabled);

        var records = Enumerable.Empty<ContentBlock>();

        records = includeDisabled
            ? CacheManager.Get(key, () =>
            {
                var zone = zoneRepository.Value.FindOne(new SearchOptions<Zone>
                {
                    Query = x =>
                        x.TenantId == tenantId &&
                        x.Name == zoneName
                });

                if (zone == null)
                {
                    zoneRepository.Value.Insert(new Zone
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        Name = zoneName
                    });
                    return Enumerable.Empty<ContentBlock>();
                }

                return pageId.HasValue
                    ? Find(new SearchOptions<ContentBlock>
                    {
                        Query = x => x.ZoneId == zone.Id && x.PageId == pageId.Value
                    })
                    : Find(new SearchOptions<ContentBlock>
                    {
                        Query = x => x.ZoneId == zone.Id && x.PageId == null
                    });
            })
            : CacheManager.Get(key, () =>
            {
                var zone = zoneRepository.Value.FindOne(new SearchOptions<Zone>
                {
                    Query = x =>
                        x.TenantId == tenantId &&
                        x.Name == zoneName
                });

                if (zone == null)
                {
                    zoneRepository.Value.Insert(new Zone
                    {
                        Id = Guid.NewGuid(),
                        TenantId = tenantId,
                        Name = zoneName
                    });
                    return Enumerable.Empty<ContentBlock>();
                }

                var list = Find(new SearchOptions<ContentBlock>
                {
                    Query = x => x.IsEnabled && x.ZoneId == zone.Id && x.PageId == null
                }).ToList();

                if (pageId.HasValue)
                {
                    list.AddRange(Find(new SearchOptions<ContentBlock>
                    {
                        Query = x => x.IsEnabled && x.ZoneId == zone.Id && x.PageId == pageId.Value
                    }));
                }

                return list;
            });

        return GetContentBlocks(records, cultureCode);
    }

    #endregion IContentBlockService Members

    private IEnumerable<IContentBlock> GetContentBlocks(IEnumerable<ContentBlock> records, string cultureCode)
    {
        string entityType = typeof(ContentBlock).FullName;
        var ids = records.Select(x => x.Id.ToString());

        var localizedRecords = localization.FindBlockValues(cultureCode, entityType, ids);

        var result = new List<IContentBlock>();
        foreach (var record in records)
        {
            IContentBlock contentBlock;
            try
            {
                // The record may be shared, so the translation is read without being written back onto it.
                string entityId = record.Id.ToString();
                string blockValues = localizedRecords.FirstOrDefault(x => x.EntityId == entityId)?.Value
                    ?? record.BlockValues
                    ?? "{}";

                var blockType = ContentBlockTypes.Resolve(record.BlockType);
                if (blockType is null)
                {
                    Logger.LogError("Content block {ContentBlockId} has an unknown type {BlockType}.", record.Id, record.BlockType);
                    continue;
                }

                contentBlock = (IContentBlock)blockValues.JsonDeserialize(blockType);
            }
            catch (Exception x)
            {
                Logger.LogError(new EventId(), x, x.Message);
                continue;
            }

            contentBlock.Id = record.Id;
            contentBlock.Title = record.Title;
            contentBlock.ZoneId = record.ZoneId;
            contentBlock.PageId = record.PageId;
            contentBlock.Order = record.Order;
            contentBlock.Enabled = record.IsEnabled;
            contentBlock.CustomDisplayType = record.CustomTemplatePath;
            result.Add(contentBlock);
        }
        return result;
    }

    protected virtual int GetTenantId() => workContext.CurrentTenant.Id;
}