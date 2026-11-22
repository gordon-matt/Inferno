using Extenso.Data.Entity;
using Localize;
using Localize.Data.Entities;

namespace Inferno.Web.ContentManagement.Localization;

/// <summary>Reads, writes and removes translated content-block values for the current tenant.</summary>
/// <param name="properties">The localization rows for block values.</param>
/// <param name="scope">Supplies the tenant scope those rows belong to.</param>
public sealed class ContentLocalization(IRepository<LocalizableProperty> properties, ILocalizationScope scope)
{
    private const string BLOCK_VALUES = "BlockValues";

    /// <summary>Loads stored block translations for the given entities.</summary>
    /// <param name="cultureCode">The culture. <see langword="null"/> or empty returns no rows.</param>
    /// <param name="entityType">The entity type name the rows were stored under.</param>
    /// <param name="entityIds">The entity ids. Empty ids are ignored, and an empty list returns no rows.</param>
    /// <returns>The matching block-value rows.</returns>
    public IReadOnlyList<LocalizableProperty> FindBlockValues(string? cultureCode, string entityType, IEnumerable<string> entityIds)
    {
        if (string.IsNullOrEmpty(cultureCode))
        {
            return [];
        }

        string[] ids = entityIds.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        string scopeId = CultureCodes.NormalizeScope(scope.GetScopeId());
        string culture = CultureCodes.Normalize(cultureCode);
        return properties.Find(new SearchOptions<LocalizableProperty>
        {
            Query = row => row.ScopeId == scopeId
                && row.CultureCode == culture
                && row.EntityType == entityType
                && ids.Contains(row.EntityId)
                && row.Property == BLOCK_VALUES,
        }).ToList();
    }

    /// <summary>Lists the cultures a block has been translated into, in culture-code order.</summary>
    /// <param name="entityType">The entity type name the rows were stored under.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns>The culture codes.</returns>
    public IReadOnlyList<string> ListBlockCultures(string entityType, string entityId)
    {
        string scopeId = CultureCodes.NormalizeScope(scope.GetScopeId());
        return properties.Find(new SearchOptions<LocalizableProperty>
        {
            Query = row => row.ScopeId == scopeId
                && row.EntityType == entityType
                && row.EntityId == entityId
                && row.Property == BLOCK_VALUES,
        }).Select(row => row.CultureCode).Order(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Gets the block's stored translation, or <see langword="null"/> when it has none for the culture.</summary>
    /// <param name="cultureCode">The culture. <see langword="null"/> or empty returns <see langword="null"/>.</param>
    /// <param name="entityType">The entity type name the row was stored under.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns>The stored value, or <see langword="null"/>.</returns>
    public string? FindBlockValue(string cultureCode, string entityType, string entityId) =>
        FindBlockValues(cultureCode, entityType, [entityId]).FirstOrDefault()?.Value;

    /// <summary>Inserts or updates the block's translation for one culture.</summary>
    /// <param name="cultureCode">The culture. It is normalized before it is stored.</param>
    /// <param name="entityType">The entity type name to store the row under.</param>
    /// <param name="entityId">The entity id.</param>
    /// <param name="value">The serialized block values.</param>
    public void SaveBlockValues(string cultureCode, string entityType, string entityId, string value)
    {
        string scopeId = CultureCodes.NormalizeScope(scope.GetScopeId());
        string culture = CultureCodes.Normalize(cultureCode);

        var existing = properties.FindOne(new SearchOptions<LocalizableProperty>
        {
            Query = row => row.ScopeId == scopeId
                && row.CultureCode == culture
                && row.EntityType == entityType
                && row.EntityId == entityId
                && row.Property == BLOCK_VALUES,
        });

        if (existing is null)
        {
            properties.Insert(new LocalizableProperty
            {
                ScopeId = scopeId,
                CultureCode = culture,
                EntityType = entityType,
                EntityId = entityId,
                Property = BLOCK_VALUES,
                Value = value,
            });
            return;
        }

        existing.Value = value;
        properties.Update(existing);
    }

    /// <summary>Deletes every stored translation for an entity, in every culture.</summary>
    /// <param name="entityType">The entity type name.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns>The number of rows deleted.</returns>
    public int DeleteFor(string entityType, string entityId)
    {
        string scopeId = CultureCodes.NormalizeScope(scope.GetScopeId());
        return properties.Delete(row => row.ScopeId == scopeId && row.EntityType == entityType && row.EntityId == entityId);
    }

    /// <summary>Deletes one culture's translation for an entity.</summary>
    /// <param name="cultureCode">The culture. It is normalized before the row is matched.</param>
    /// <param name="entityType">The entity type name.</param>
    /// <param name="entityId">The entity id.</param>
    /// <returns>The number of rows deleted.</returns>
    public int DeleteFor(string cultureCode, string entityType, string entityId)
    {
        string scopeId = CultureCodes.NormalizeScope(scope.GetScopeId());
        string culture = CultureCodes.Normalize(cultureCode);
        return properties.Delete(row =>
            row.ScopeId == scopeId
            && row.CultureCode == culture
            && row.EntityType == entityType
            && row.EntityId == entityId);
    }
}
