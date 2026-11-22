using Ardalis.Result;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;
using Inferno.Web.ContentManagement.Localization;
using Localize;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

/// <summary>A kind of block an editor can add. <see cref="TypeName"/> is what gets stored on the block.</summary>
/// <param name="TypeName">The type name stored on the block.</param>
/// <param name="Name">The name shown to the editor.</param>
/// <param name="EditorType">The editor component for this type.</param>
public sealed record BlockTypeOption(string TypeName, string Name, Type EditorType);

/// <summary>A zone an editor can assign a block to.</summary>
/// <param name="Id">The zone identifier.</param>
/// <param name="Name">The zone name.</param>
public sealed record ZoneOption(Guid Id, string Name);

/// <summary>A zone with how many blocks sit in it, so a zone that is still in use is not removed by mistake.</summary>
/// <param name="Id">The zone identifier.</param>
/// <param name="Name">The zone name.</param>
/// <param name="BlockCount">How many blocks are in the zone, including blocks assigned to pages.</param>
public sealed record ZoneSummary(Guid Id, string Name, int BlockCount);

/// <summary>A block's values in one culture. <see cref="Exists"/> is false when the block has not been translated yet and the values are the default ones.</summary>
/// <param name="CultureCode">The culture the values belong to.</param>
/// <param name="BlockValues">The serialized values for that culture, or the default values when <paramref name="Exists"/> is <see langword="false"/>.</param>
/// <param name="Exists"><see langword="true"/> when a translation is stored for the culture.</param>
public sealed record BlockTranslation(string CultureCode, string BlockValues, bool Exists);

/// <summary>Edits zones, content blocks, and block translations for one tenant.</summary>
public interface IContentBlockAdminService
{
    /// <summary>Lists the registered block types, one per stored type name, ordered by display name.</summary>
    /// <returns>The types an editor can add.</returns>
    IReadOnlyList<BlockTypeOption> GetBlockTypes();

    /// <summary>Lists the tenant's zones by name.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <returns>The zones.</returns>
    Task<Result<IReadOnlyList<ZoneOption>>> ListZonesAsync(int tenantId);

    /// <summary>Lists the tenant's zones with how many blocks each one holds.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <returns>The zones and their block counts.</returns>
    Task<Result<IReadOnlyList<ZoneSummary>>> ListZoneSummariesAsync(int tenantId);

    /// <summary>Adds a zone when <paramref name="id"/> is empty, otherwise renames it. Names are unique within the tenant.</summary>
    /// <param name="tenantId">The tenant that owns the zone.</param>
    /// <param name="id"><see cref="Guid.Empty"/> adds a zone. Any other id renames that zone.</param>
    /// <param name="name">The zone name. Leading and trailing whitespace is removed.</param>
    /// <returns>
    /// The saved zone. Invalid when the name is missing, longer than 255 characters, or already used.
    /// Not found when the zone is missing or belongs to another tenant.
    /// </returns>
    Task<Result<ZoneOption>> SaveZoneAsync(int tenantId, Guid id, string name);

    /// <summary>Removes an empty zone. A zone that still holds blocks is refused.</summary>
    /// <param name="tenantId">The tenant that owns the zone.</param>
    /// <param name="id">The zone to remove.</param>
    /// <returns>Success, not found, or a conflict when the zone still has blocks.</returns>
    Task<Result> DeleteZoneAsync(int tenantId, Guid id);

    /// <summary>
    /// Lists blocks in the tenant's zones: the site-wide ones when <paramref name="pageId"/> is <see langword="null"/>,
    /// otherwise only the ones assigned to that page.
    /// </summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="search">Matches the title or block name. Whitespace lists every block in the filter.</param>
    /// <param name="zoneId">When set, limits the list to that zone. A zone from another tenant yields no blocks.</param>
    /// <param name="pageId">The page, or <see langword="null"/> for blocks that are not assigned to a page.</param>
    /// <returns>The blocks, ordered by position and then title.</returns>
    Task<Result<IReadOnlyList<ContentBlock>>> ListAsync(int tenantId, string search, Guid? zoneId, Guid? pageId = null);

    /// <summary>Gets the name of a page in the tenant, for headings on the page's block list.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="pageId">The page. <see cref="Guid.Empty"/> is not found.</param>
    /// <returns>The page name, or not found when the page is missing or belongs to another tenant.</returns>
    Task<Result<string>> GetPageNameAsync(int tenantId, Guid pageId);

    /// <summary>Gets the block's values for a culture, or its default values when there is no translation yet.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="blockId">The block.</param>
    /// <param name="cultureCode">The culture. An unknown or empty culture is invalid.</param>
    /// <returns>The translation. <see cref="BlockTranslation.Exists"/> is <see langword="false"/> when the values are the block's defaults.</returns>
    Task<Result<BlockTranslation>> GetTranslationAsync(int tenantId, Guid blockId, string cultureCode);

    /// <summary>Lists the cultures the block has been translated into, in culture-code order.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="blockId">The block.</param>
    /// <returns>The culture codes, or not found when the block is missing.</returns>
    Task<Result<IReadOnlyList<string>>> ListTranslationCulturesAsync(int tenantId, Guid blockId);

    /// <summary>Stores the block's values for one culture and drops the cached block lists.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="blockId">The block.</param>
    /// <param name="cultureCode">The culture. An unknown or empty culture is invalid.</param>
    /// <param name="blockValues">The serialized values. Empty is stored as an empty JSON object.</param>
    /// <returns>Success, not found, invalid, or an error when the translation cannot be saved.</returns>
    Task<Result> SaveTranslationAsync(int tenantId, Guid blockId, string cultureCode, string blockValues);

    /// <summary>Removes a translation, so the block shows its default values in that culture again.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="blockId">The block.</param>
    /// <param name="cultureCode">The culture. An unknown or empty culture is invalid.</param>
    /// <returns>Success, not found, invalid, or an error when the translation cannot be deleted.</returns>
    Task<Result> DeleteTranslationAsync(int tenantId, Guid blockId, string cultureCode);

    /// <summary>Loads a block that belongs to one of the tenant's zones.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="id">The block. <see cref="Guid.Empty"/> is not found.</param>
    /// <returns>The block, or not found.</returns>
    Task<Result<ContentBlock>> GetAsync(int tenantId, Guid id);

    /// <summary>Inserts when <see cref="ContentBlock.Id"/> is empty, otherwise updates.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="block">The values to save.</param>
    /// <returns>
    /// The saved block. Invalid when the title, type, or zone is missing.
    /// Not found when an existing block is missing, or when a new block names a page the tenant does not have.
    /// </returns>
    /// <remarks>
    /// An update changes the title, zone, order, enabled flag, custom template path, and values.
    /// The block type and page stay as they were. A new block stores an empty JSON object for its values.
    /// </remarks>
    Task<Result<ContentBlock>> SaveAsync(int tenantId, ContentBlock block);

    /// <summary>Deletes the block and its translations.</summary>
    /// <param name="tenantId">The tenant.</param>
    /// <param name="id">The block.</param>
    /// <returns>Success, not found, or an error when the block cannot be deleted.</returns>
    Task<Result> DeleteAsync(int tenantId, Guid id);
}

/// <inheritdoc cref="IContentBlockAdminService" />
/// <param name="blockTypes">The registered block types an editor can add.</param>
/// <param name="zoneService">Loads and saves zones.</param>
/// <param name="contentBlockService">Loads, saves, and deletes blocks, and clears their cache.</param>
/// <param name="pageService">Resolves a page name and checks that a page belongs to the tenant.</param>
/// <param name="localization">Reads and writes translated block values.</param>
/// <param name="logger">Records save and delete failures.</param>
public class ContentBlockAdminService(
    IEnumerable<IContentBlock> blockTypes,
    IZoneService zoneService,
    IContentBlockService contentBlockService,
    IPageService pageService,
    ContentLocalization localization,
    ILogger<ContentBlockAdminService> logger) : IContentBlockAdminService
{
    private const string EMPTY_BLOCK_VALUES = "{}";
    private const int ZONE_NAME_MAX_LENGTH = 255;

    private static readonly string BlockEntityType = typeof(ContentBlock).FullName;

    /// <inheritdoc />
    public IReadOnlyList<BlockTypeOption> GetBlockTypes() => blockTypes
        .Select(block => new BlockTypeOption(block.GetType().FullName, block.Name, block.EditorType))
        .DistinctBy(option => option.TypeName)
        .OrderBy(option => option.Name)
        .ToList();

    /// <inheritdoc />
    public Task<Result<IReadOnlyList<ZoneOption>>> ListZonesAsync(int tenantId)
    {
        IReadOnlyList<ZoneOption> zones = TenantZones(tenantId)
            .Select(zone => new ZoneOption(zone.Id, zone.Name))
            .ToList();

        return Task.FromResult(Result.Success(zones));
    }

    /// <inheritdoc />
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

        IReadOnlyList<ZoneSummary> summaries = zones
            .Select(zone => new ZoneSummary(zone.Id, zone.Name, counts.GetValueOrDefault(zone.Id)))
            .ToList();

        return Task.FromResult(Result.Success(summaries));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Result<string>> GetPageNameAsync(int tenantId, Guid pageId)
    {
        var page = pageId == Guid.Empty ? null : await pageService.FindOneAsync(pageId);
        return page is null || page.TenantId != tenantId ? Result<string>.NotFound() : Result.Success(page.Name);
    }

    /// <inheritdoc />
    public async Task<Result<ContentBlock>> GetAsync(int tenantId, Guid id)
    {
        var block = await FindBlockAsync(tenantId, id);
        return block is null ? Result<ContentBlock>.NotFound() : Result.Success(block);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<string>>> ListTranslationCulturesAsync(int tenantId, Guid blockId)
    {
        var block = await FindBlockAsync(tenantId, blockId);
        if (block is null)
        {
            return Result<IReadOnlyList<string>>.NotFound();
        }

        return Result.Success(localization.ListBlockCultures(BlockEntityType, block.Id.ToString()));
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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


    private void ResetBlockCaches()
    {
        contentBlockService.ResetCache();
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
