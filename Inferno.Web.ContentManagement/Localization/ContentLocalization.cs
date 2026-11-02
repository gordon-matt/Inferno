using Extenso.Data.Entity;
using Localize;
using Localize.Data.Entities;

namespace Inferno.Web.ContentManagement.Localization;

/// <summary>Reads and removes translated content-block values for the current tenant.</summary>
public sealed class ContentLocalization(IRepository<LocalizableProperty> properties, ILocalizationScope scope)
{
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
                && row.Property == "BlockValues",
        }).ToList();
    }

    public int DeleteFor(string entityType, string entityId)
    {
        string scopeId = CultureCodes.NormalizeScope(scope.GetScopeId());
        return properties.Delete(row => row.ScopeId == scopeId && row.EntityType == entityType && row.EntityId == entityId);
    }
}
