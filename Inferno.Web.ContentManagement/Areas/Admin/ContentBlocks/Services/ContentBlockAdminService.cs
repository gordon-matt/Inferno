using Ardalis.Result;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.ContentManagement.Localization;
using Localize;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

/// <summary>A kind of block an editor can add. <see cref="TypeName"/> is what gets stored on the block.</summary>
public sealed record BlockTypeOption(string TypeName, string Name, Type EditorType);

public sealed record ZoneOption(Guid Id, string Name);

/// <summary>A zone with how many blocks sit in it, so a zone that is still in use is not removed by mistake.</summary>
public sealed record ZoneSummary(Guid Id, string Name, int BlockCount);

/// <summary>A block's values in one culture. <see cref="Exists"/> is false when the block has not been translated yet and the values are the default ones.</summary>
public sealed record BlockTranslation(string CultureCode, string BlockValues, bool Exists);

public interface IContentBlockAdminService
{
    IReadOnlyList<BlockTypeOption> GetBlockTypes();

    Task<Result<IReadOnlyList<ZoneOption>>> ListZonesAsync(int tenantId);

    Task<Result<IReadOnlyList<ZoneSummary>>> ListZoneSummariesAsync(int tenantId);

    /// <summary>Adds a zone when <paramref name="id"/> is empty, otherwise renames it. Names are unique within the tenant.</summary>
    Task<Result<ZoneOption>> SaveZoneAsync(int tenantId, Guid id, string name);

    /// <summary>Removes an empty zone. A zone that still holds blocks is refused.</summary>
    Task<Result> DeleteZoneAsync(int tenantId, Guid id);

    /// <summary>
    /// Blocks in the tenant's zones: the site-wide ones when <paramref name="pageId"/> is null,
    /// otherwise only the ones assigned to that page.
    /// </summary>
    Task<Result<IReadOnlyList<ContentBlock>>> ListAsync(int tenantId, string search, Guid? zoneId, Guid? pageId = null);

    /// <summary>The name of a page in the tenant, for headings on the page's block list.</summary>
    Task<Result<string>> GetPageNameAsync(int tenantId, Guid pageId);

    /// <summary>The block's values for a culture, or its default values when there is no translation yet.</summary>
    Task<Result<BlockTranslation>> GetTranslationAsync(int tenantId, Guid blockId, string cultureCode);

    Task<Result<IReadOnlyList<string>>> ListTranslationCulturesAsync(int tenantId, Guid blockId);

    Task<Result> SaveTranslationAsync(int tenantId, Guid blockId, string cultureCode, string blockValues);

    /// <summary>Removes a translation, so the block shows its default values in that culture again.</summary>
    Task<Result> DeleteTranslationAsync(int tenantId, Guid blockId, string cultureCode);

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
    IPageService pageService,
    ContentLocalization localization,
    ILogger<ContentBlockAdminService> logger) : IContentBlockAdminService
{
    private const string EMPTY_BLOCK_VALUES = "{}";
    private const int ZONE_NAME_MAX_LENGTH = 255;

    private static readonly string BlockEntityType = typeof(ContentBlock).FullName;

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

    public Task<Result<IReadOnlyList<ZoneSummary>>> ListZoneSummariesAsync(int tenantId)
    {
        var zones = TenantZones(tenantId);
        var zoneIds = zones.Select(zone => zone.Id).ToList();

        var counts = new Dictionary<Guid, int>();
        using (var connection = contentBlockService.OpenConnection())
        {
            var grouped = connection.Query(block => zoneIds.Contains(block.ZoneId))
                .GroupBy(block => block.ZoneId)
                .Select(group => new { ZoneId = group.Key, Count = group.Count() })
                .ToList();

            foreach (var entry in grouped)
            {
                counts[entry.ZoneId] = entry.Count;
            }
        }

        using (var connection = entityTypeContentBlockService.OpenConnection())
        {
            var grouped = connection.Query(block => zoneIds.Contains(block.ZoneId))
                .GroupBy(block => block.ZoneId)
                .Select(group => new { ZoneId = group.Key, Count = group.Count() })
                .ToList();

            foreach (var entry in grouped)
            {
                counts[entry.ZoneId] = counts.GetValueOrDefault(entry.ZoneId) + entry.Count;
            }
        }

        IReadOnlyList<ZoneSummary> summaries = zones
            .Select(zone => new ZoneSummary(zone.Id, zone.Name, counts.GetValueOrDefault(zone.Id)))
            .ToList();

        return Task.FromResult(Result.Success(summaries));
    }

    public async Task<Result<ZoneOption>> SaveZoneAsync(int tenantId, Guid id, string name)
    {
        string trimmed = name?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return Result<ZoneOption>.Invalid(new ValidationError { Identifier = nameof(Zone.Name), ErrorMessage = "Enter a name." });
        }

        if (trimmed.Length > ZONE_NAME_MAX_LENGTH)
        {
            return Result<ZoneOption>.Invalid(new ValidationError { Identifier = nameof(Zone.Name), ErrorMessage = $"Use {ZONE_NAME_MAX_LENGTH} characters or fewer." });
        }

        if (TenantZones(tenantId).Any(zone => zone.Id != id && string.Equals(zone.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<ZoneOption>.Invalid(new ValidationError { Identifier = nameof(Zone.Name), ErrorMessage = "A zone with that name already exists." });
        }

        try
        {
            if (id != Guid.Empty)
            {
                var existing = await zoneService.FindOneAsync(id);
                if (existing is null || existing.TenantId != tenantId)
                {
                    return Result<ZoneOption>.NotFound();
                }

                existing.Name = trimmed;
                await zoneService.UpdateAsync(existing);
                ResetBlockCaches();
                return Result.Success(new ZoneOption(existing.Id, existing.Name));
            }

            var created = await zoneService.InsertAsync(new Zone { Id = Guid.NewGuid(), TenantId = tenantId, Name = trimmed });
            return Result.Success(new ZoneOption(created.Id, created.Name));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save zone {ZoneId} for tenant {TenantId}", id, tenantId);
            return Result<ZoneOption>.Error("The zone could not be saved.");
        }
    }

    public async Task<Result> DeleteZoneAsync(int tenantId, Guid id)
    {
        var zone = await zoneService.FindOneAsync(id);
        if (zone is null || zone.TenantId != tenantId)
        {
            return Result.NotFound();
        }

        var summaries = await ListZoneSummariesAsync(tenantId);
        var summary = summaries.Value.FirstOrDefault(candidate => candidate.Id == id);
        if (summary is { BlockCount: > 0 })
        {
            return Result.Conflict($"The zone \"{zone.Name}\" still has {summary.BlockCount} block(s). Delete or move them first.");
        }

        try
        {
            await zoneService.DeleteAsync(zone);
            ResetBlockCaches();
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not delete zone {ZoneId} for tenant {TenantId}", id, tenantId);
            return Result.Error("The zone could not be deleted.");
        }
    }

    public Task<Result<IReadOnlyList<ContentBlock>>> ListAsync(int tenantId, string search, Guid? zoneId, Guid? pageId = null)
    {
        var zoneIds = ZoneFilter(tenantId, zoneId);

        using var connection = contentBlockService.OpenConnection();
        var query = connection.Query(block => block.PageId == pageId && zoneIds.Contains(block.ZoneId));
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(block => block.Title.Contains(term) || block.BlockName.Contains(term));
        }

        IReadOnlyList<ContentBlock> blocks = query.OrderBy(block => block.Order).ThenBy(block => block.Title).ToList();
        return Task.FromResult(Result.Success(blocks));
    }

    public async Task<Result<string>> GetPageNameAsync(int tenantId, Guid pageId)
    {
        var page = pageId == Guid.Empty ? null : await pageService.FindOneAsync(pageId);
        return page is null || page.TenantId != tenantId ? Result<string>.NotFound() : Result.Success(page.Name);
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

        if (block.Id == Guid.Empty && block.PageId.HasValue && !(await GetPageNameAsync(tenantId, block.PageId.Value)).IsSuccess)
        {
            return Result<ContentBlock>.NotFound();
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
                PageId = block.PageId,
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

    public async Task<Result<BlockTranslation>> GetTranslationAsync(int tenantId, Guid blockId, string cultureCode)
    {
        var block = await FindBlockAsync(tenantId, blockId);
        if (block is null)
        {
            return Result<BlockTranslation>.NotFound();
        }

        var invalid = ValidateCulture(cultureCode, out string culture);
        if (invalid is not null)
        {
            return Result<BlockTranslation>.Invalid(invalid);
        }

        string stored = localization.FindBlockValue(culture, BlockEntityType, block.Id.ToString());
        return Result.Success(new BlockTranslation(culture, stored ?? block.BlockValues ?? EMPTY_BLOCK_VALUES, stored is not null));
    }

    public async Task<Result<IReadOnlyList<string>>> ListTranslationCulturesAsync(int tenantId, Guid blockId)
    {
        var block = await FindBlockAsync(tenantId, blockId);
        if (block is null)
        {
            return Result<IReadOnlyList<string>>.NotFound();
        }

        return Result.Success(localization.ListBlockCultures(BlockEntityType, block.Id.ToString()));
    }

    public async Task<Result> SaveTranslationAsync(int tenantId, Guid blockId, string cultureCode, string blockValues)
    {
        var block = await FindBlockAsync(tenantId, blockId);
        if (block is null)
        {
            return Result.NotFound();
        }

        var invalid = ValidateCulture(cultureCode, out string culture);
        if (invalid is not null)
        {
            return Result.Invalid(invalid);
        }

        try
        {
            localization.SaveBlockValues(culture, BlockEntityType, block.Id.ToString(), string.IsNullOrEmpty(blockValues) ? EMPTY_BLOCK_VALUES : blockValues);
            contentBlockService.ResetCache();
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not save the {CultureCode} translation of content block {ContentBlockId}", culture, blockId);
            return Result.Error("The translation could not be saved.");
        }
    }

    public async Task<Result> DeleteTranslationAsync(int tenantId, Guid blockId, string cultureCode)
    {
        var block = await FindBlockAsync(tenantId, blockId);
        if (block is null)
        {
            return Result.NotFound();
        }

        var invalid = ValidateCulture(cultureCode, out string culture);
        if (invalid is not null)
        {
            return Result.Invalid(invalid);
        }

        try
        {
            localization.DeleteFor(culture, BlockEntityType, block.Id.ToString());
            contentBlockService.ResetCache();
            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not delete the {CultureCode} translation of content block {ContentBlockId}", culture, blockId);
            return Result.Error("The translation could not be deleted.");
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

    private void ResetBlockCaches()
    {
        contentBlockService.ResetCache();
        entityTypeContentBlockService.ResetCache();
    }

    private static ValidationError ValidateCulture(string cultureCode, out string culture)
    {
        culture = CultureCodes.Normalize(cultureCode);
        return culture.Length == 0 || !CultureCodes.IsKnown(culture)
            ? new ValidationError { Identifier = "CultureCode", ErrorMessage = "Choose a language." }
            : null;
    }

    private static string NullIfBlank(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
