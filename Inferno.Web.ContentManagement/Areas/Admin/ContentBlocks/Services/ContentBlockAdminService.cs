using Ardalis.Result;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

/// <summary>A kind of block an editor can add. <see cref="TypeName"/> is what gets stored on the block.</summary>
public sealed record BlockTypeOption(string TypeName, string Name, Type EditorType);

public sealed record ZoneOption(Guid Id, string Name);

public interface IContentBlockAdminService
{
    IReadOnlyList<BlockTypeOption> GetBlockTypes();

    Task<Result<IReadOnlyList<ZoneOption>>> ListZonesAsync(int tenantId);

    /// <summary>Site-wide blocks (those not tied to a page) in the tenant's zones.</summary>
    Task<Result<IReadOnlyList<ContentBlock>>> ListAsync(int tenantId, string search, Guid? zoneId);

    Task<Result<ContentBlock>> GetAsync(int tenantId, Guid id);

    /// <summary>Inserts when <see cref="ContentBlock.Id"/> is empty, otherwise updates.</summary>
    Task<Result<ContentBlock>> SaveAsync(int tenantId, ContentBlock block);

    Task<Result> DeleteAsync(int tenantId, Guid id);

    Task<Result<IReadOnlyList<EntityTypeContentBlock>>> ListEntityBlocksAsync(int tenantId, string search, Guid? zoneId);

    Task<Result<EntityTypeContentBlock>> GetEntityBlockAsync(int tenantId, Guid id);

    /// <summary>Inserts when <see cref="EntityTypeContentBlock.Id"/> is empty, otherwise updates.</summary>
    Task<Result<EntityTypeContentBlock>> SaveEntityBlockAsync(int tenantId, EntityTypeContentBlock block);

    Task<Result> DeleteEntityBlockAsync(int tenantId, Guid id);
}

public class ContentBlockAdminService(
    IEnumerable<IContentBlock> blockTypes,
    IZoneService zoneService,
    IContentBlockService contentBlockService,
    IEntityTypeContentBlockService entityTypeContentBlockService,
    ILogger<ContentBlockAdminService> logger) : IContentBlockAdminService
{
    private const string EMPTY_BLOCK_VALUES = "{}";

    public IReadOnlyList<BlockTypeOption> GetBlockTypes() => blockTypes
        .Select(block => new BlockTypeOption(block.GetType().FullName, block.Name, block.EditorType))
        .DistinctBy(option => option.TypeName)
        .OrderBy(option => option.Name)
        .ToList();

    public Task<Result<IReadOnlyList<ZoneOption>>> ListZonesAsync(int tenantId)
    {
        IReadOnlyList<ZoneOption> zones = TenantZones(tenantId)
            .Select(zone => new ZoneOption(zone.Id, zone.Name))
            .ToList();

        return Task.FromResult(Result.Success(zones));
    }

    public Task<Result<IReadOnlyList<ContentBlock>>> ListAsync(int tenantId, string search, Guid? zoneId)
    {
        var zoneIds = ZoneFilter(tenantId, zoneId);

        using var connection = contentBlockService.OpenConnection();
        var query = connection.Query(block => block.PageId == null && zoneIds.Contains(block.ZoneId));
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(block => block.Title.Contains(term) || block.BlockName.Contains(term));
        }

        IReadOnlyList<ContentBlock> blocks = query.OrderBy(block => block.Order).ThenBy(block => block.Title).ToList();
        return Task.FromResult(Result.Success(blocks));
    }

    public async Task<Result<ContentBlock>> GetAsync(int tenantId, Guid id)
    {
        var block = await FindBlockAsync(tenantId, id);
        return block is null ? Result<ContentBlock>.NotFound() : Result.Success(block);
    }

    public async Task<Result<ContentBlock>> SaveAsync(int tenantId, ContentBlock block)
    {
        var errors = Validate(tenantId, block.Title, block.BlockType, block.ZoneId, out var blockType);
        if (errors.Count > 0)
        {
            return Result<ContentBlock>.Invalid(errors);
        }

        try
        {
            if (block.Id != Guid.Empty)
            {
                var existing = await FindBlockAsync(tenantId, block.Id);
                if (existing is null)
                {
                    return Result<ContentBlock>.NotFound();
                }

                existing.Title = block.Title.Trim();
                existing.ZoneId = block.ZoneId;
                existing.Order = block.Order;
                existing.IsEnabled = block.IsEnabled;
                existing.CustomTemplatePath = NullIfBlank(block.CustomTemplatePath);
                existing.BlockValues = string.IsNullOrEmpty(block.BlockValues) ? EMPTY_BLOCK_VALUES : block.BlockValues;
                await contentBlockService.UpdateAsync(existing);
                return Result.Success(existing);
            }

            var created = await contentBlockService.InsertAsync(new ContentBlock
            {
                Id = Guid.NewGuid(),
                Title = block.Title.Trim(),
                BlockType = blockType.TypeName,
                BlockName = blockType.Name,
                ZoneId = block.ZoneId,
                Order = block.Order,
                IsEnabled = block.IsEnabled,
                CustomTemplatePath = NullIfBlank(block.CustomTemplatePath),
                BlockValues = EMPTY_BLOCK_VALUES
            });

            return Result.Success(created);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save content block {ContentBlockId}", block.Id);
            return Result<ContentBlock>.Error("The content block could not be saved.");
        }
    }

    public async Task<Result> DeleteAsync(int tenantId, Guid id)
    {
        var block = await FindBlockAsync(tenantId, id);
        if (block is null)
        {
            return Result.NotFound();
        }

        try
        {
            // The synchronous overload also removes the block's translations.
            contentBlockService.Delete(block);
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not delete content block {ContentBlockId}", id);
            return Result.Error("The content block could not be deleted.");
        }
    }

    public Task<Result<IReadOnlyList<EntityTypeContentBlock>>> ListEntityBlocksAsync(int tenantId, string search, Guid? zoneId)
    {
        var zoneIds = ZoneFilter(tenantId, zoneId);

        using var connection = entityTypeContentBlockService.OpenConnection();
        var query = connection.Query(block => zoneIds.Contains(block.ZoneId));
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(block =>
                block.Title.Contains(term) ||
                block.BlockName.Contains(term) ||
                block.EntityType.Contains(term) ||
                block.EntityId.Contains(term));
        }

        IReadOnlyList<EntityTypeContentBlock> blocks = query
            .OrderBy(block => block.EntityType)
            .ThenBy(block => block.EntityId)
            .ThenBy(block => block.Order)
            .ToList();

        return Task.FromResult(Result.Success(blocks));
    }

    public async Task<Result<EntityTypeContentBlock>> GetEntityBlockAsync(int tenantId, Guid id)
    {
        var block = await FindEntityBlockAsync(tenantId, id);
        return block is null ? Result<EntityTypeContentBlock>.NotFound() : Result.Success(block);
    }

    public async Task<Result<EntityTypeContentBlock>> SaveEntityBlockAsync(int tenantId, EntityTypeContentBlock block)
    {
        var errors = Validate(tenantId, block.Title, block.BlockType, block.ZoneId, out var blockType);
        if (string.IsNullOrWhiteSpace(block.EntityType))
        {
            errors.Add(new ValidationError { Identifier = nameof(EntityTypeContentBlock.EntityType), ErrorMessage = "Enter the entity type." });
        }

        if (string.IsNullOrWhiteSpace(block.EntityId))
        {
            errors.Add(new ValidationError { Identifier = nameof(EntityTypeContentBlock.EntityId), ErrorMessage = "Enter the entity ID." });
        }

        if (errors.Count > 0)
        {
            return Result<EntityTypeContentBlock>.Invalid(errors);
        }

        try
        {
            if (block.Id != Guid.Empty)
            {
                var existing = await FindEntityBlockAsync(tenantId, block.Id);
                if (existing is null)
                {
                    return Result<EntityTypeContentBlock>.NotFound();
                }

                existing.Title = block.Title.Trim();
                existing.EntityType = block.EntityType.Trim();
                existing.EntityId = block.EntityId.Trim();
                existing.ZoneId = block.ZoneId;
                existing.Order = block.Order;
                existing.IsEnabled = block.IsEnabled;
                existing.CustomTemplatePath = NullIfBlank(block.CustomTemplatePath);
                existing.BlockValues = string.IsNullOrEmpty(block.BlockValues) ? EMPTY_BLOCK_VALUES : block.BlockValues;
                await entityTypeContentBlockService.UpdateAsync(existing);
                return Result.Success(existing);
            }

            var created = await entityTypeContentBlockService.InsertAsync(new EntityTypeContentBlock
            {
                Id = Guid.NewGuid(),
                Title = block.Title.Trim(),
                EntityType = block.EntityType.Trim(),
                EntityId = block.EntityId.Trim(),
                BlockType = blockType.TypeName,
                BlockName = blockType.Name,
                ZoneId = block.ZoneId,
                Order = block.Order,
                IsEnabled = block.IsEnabled,
                CustomTemplatePath = NullIfBlank(block.CustomTemplatePath),
                BlockValues = EMPTY_BLOCK_VALUES
            });

            return Result.Success(created);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save entity type content block {ContentBlockId}", block.Id);
            return Result<EntityTypeContentBlock>.Error("The content block could not be saved.");
        }
    }

    public async Task<Result> DeleteEntityBlockAsync(int tenantId, Guid id)
    {
        var block = await FindEntityBlockAsync(tenantId, id);
        if (block is null)
        {
            return Result.NotFound();
        }

        try
        {
            // The synchronous overload also removes the block's translations.
            entityTypeContentBlockService.Delete(block);
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not delete entity type content block {ContentBlockId}", id);
            return Result.Error("The content block could not be deleted.");
        }
    }

    private List<Zone> TenantZones(int tenantId)
    {
        using var connection = zoneService.OpenConnection();
        return connection.Query(zone => zone.TenantId == tenantId).OrderBy(zone => zone.Name).ToList();
    }

    private List<Guid> ZoneFilter(int tenantId, Guid? zoneId)
    {
        var zoneIds = TenantZones(tenantId).Select(zone => zone.Id).ToList();
        return zoneId.HasValue ? zoneIds.Where(id => id == zoneId.Value).ToList() : zoneIds;
    }

    private bool IsTenantZone(int tenantId, Guid zoneId) => TenantZones(tenantId).Any(zone => zone.Id == zoneId);

    private List<ValidationError> Validate(int tenantId, string title, string typeName, Guid zoneId, out BlockTypeOption blockType)
    {
        var errors = new List<ValidationError>();
        if (string.IsNullOrWhiteSpace(title))
        {
            errors.Add(new ValidationError { Identifier = nameof(ContentBlock.Title), ErrorMessage = "Enter a title." });
        }

        blockType = GetBlockTypes().FirstOrDefault(option => option.TypeName == typeName);
        if (blockType is null)
        {
            errors.Add(new ValidationError { Identifier = nameof(ContentBlock.BlockType), ErrorMessage = "Choose a block type." });
        }

        if (zoneId == Guid.Empty || !IsTenantZone(tenantId, zoneId))
        {
            errors.Add(new ValidationError { Identifier = nameof(ContentBlock.ZoneId), ErrorMessage = "Choose a zone." });
        }

        return errors;
    }

    private async Task<ContentBlock> FindBlockAsync(int tenantId, Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var block = await contentBlockService.FindOneAsync(id);
        return block is not null && IsTenantZone(tenantId, block.ZoneId) ? block : null;
    }

    private async Task<EntityTypeContentBlock> FindEntityBlockAsync(int tenantId, Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var block = await entityTypeContentBlockService.FindOneAsync(id);
        return block is not null && IsTenantZone(tenantId, block.ZoneId) ? block : null;
    }

    private static string NullIfBlank(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
