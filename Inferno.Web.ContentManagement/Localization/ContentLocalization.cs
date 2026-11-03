using Extenso.Data.Entity;
using Localize;
using Localize.Data.Entities;

namespace Inferno.Web.ContentManagement.Localization;

/// <summary>Reads, writes and removes translated content-block values for the current tenant.</summary>
public sealed class ContentLocalization(IRepository<LocalizableProperty> properties, ILocalizationScope scope)
{
    private const string BLOCK_VALUES = "BlockValues";

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

    /// <summary>The cultures a block has been translated into, in code order.</summary>
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

    /// <summary>The block's stored translation, or null when it has none for the culture.</summary>
    public string? FindBlockValue(string cultureCode, string entityType, string entityId) =>
        FindBlockValues(cultureCode, entityType, [entityId]).FirstOrDefault()?.Value;

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

    public int DeleteFor(string entityType, string entityId)
    {
        string scopeId = CultureCodes.NormalizeScope(scope.GetScopeId());
        return properties.Delete(row => row.ScopeId == scopeId && row.EntityType == entityType && row.EntityId == entityId);
    }

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
