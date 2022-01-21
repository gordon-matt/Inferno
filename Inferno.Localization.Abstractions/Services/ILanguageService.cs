using Inferno.Data.Services;
using LanguageEntity = Inferno.Localization.Entities.Language;

namespace Inferno.Localization.Services
{
    public interface ILanguageService : IGenericDataService<LanguageEntity>
    {
        IEnumerable<LanguageEntity> GetActiveLanguages(int tenantId);

        bool CheckIfRightToLeft(int tenantId, string cultureCode);
    }
}