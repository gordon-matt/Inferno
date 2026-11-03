using Extenso;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Localization;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

public interface IEntityTypeContentBlockService : IGenericDataService<EntityTypeContentBlock>
{
    IEnumerable<IContentBlock> GetContentBlocks(string entityType, string entityId, string zoneName, string cultureCode, bool includeDisabled = false);

    /// <summary>Drops the cached block lists, for changes that do not go through this service.</summary>
    void ResetCache();
}

public class EntityTypeContentBlockService : GenericDataService<EntityTypeContentBlock>, IEntityTypeContentBlockService
{
    private readonly Lazy<IRepository<Zone>> zoneRepository;
    private readonly ContentLocalization localization;
    private readonly IWorkContext workContext;

    public EntityTypeContentBlockService(
        ICacheManager cacheManager,
        IRepository<EntityTypeContentBlock> repository,
        Lazy<IRepository<Zone>> zoneRepository,
        ContentLocalization localization,
        IWorkContext workContext)
        : base(cacheManager, repository)
    {
        this.zoneRepository = zoneRepository;
        this.localization = localization;
        this.workContext = workContext;
    }

    //TODO: Override other Delete() methods with similar logic to this one
    public override int Delete(EntityTypeContentBlock entity)
    {
        string entityType = typeof(EntityTypeContentBlock).FullName;
        string entityId = entity.Id.ToString();

        int rowsAffected = localization.DeleteFor(entityType, entityId);
        rowsAffected += base.Delete(entity);

        return rowsAffected;
    }

    #region IEntityTypeContentBlockService Members

    public IEnumerable<IContentBlock> GetContentBlocks(string entityType, string entityId, string zoneName, string cultureCode, bool includeDisabled = false)
    {
        int tenantId = GetTenantId();

        string key = string.Format(
            "Repository_EntityTypeContentBlocks_GetContentBlocks_{0}_{1}_{2}_{3}_{4}_{5}",
            tenantId,
            entityType,
            cultureCode,
            entityId,
            zoneName,
            includeDisabled);

        var records = CacheManager.Get(key, () =>
        {
            var zone = zoneRepository.Value.FindOne(new SearchOptions<Zone>
            {
                Query = x => x.TenantId == tenantId && x.Name == zoneName
            });

            if (zone == null)
            {
                zoneRepository.Value.Insert(new Zone
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    Name = zoneName
                });
                return Enumerable.Empty<EntityTypeContentBlock>();
            }

            using var connection = OpenConnection();
            var query = connection.Query(x =>
                x.EntityType == entityType &&
                x.EntityId == entityId &&
                x.ZoneId == zone.Id);

            if (!includeDisabled)
            {
                query = query.Where(x => x.IsEnabled);
            }

            return query.ToList();
        });

        return GetContentBlocks(records, cultureCode);
    }

    #endregion IEntityTypeContentBlockService Members

    public void ResetCache() => ClearCache();

    protected override void ClearCache()
    {
        base.ClearCache();
        CacheManager.RemoveByPattern("Repository_EntityTypeContentBlocks_GetContentBlocks_.*");
    }

    private IEnumerable<IContentBlock> GetContentBlocks(IEnumerable<EntityTypeContentBlock> records, string cultureCode)
    {
        string entityType = typeof(EntityTypeContentBlock).FullName;
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

                var blockType = Type.GetType(record.BlockType);
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
            contentBlock.Order = record.Order;
            contentBlock.Enabled = record.IsEnabled;
            contentBlock.CustomDisplayType = record.CustomTemplatePath;
            result.Add(contentBlock);
        }
        return result;
    }

    protected virtual int GetTenantId() => workContext.CurrentTenant.Id;
}