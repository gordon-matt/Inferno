using Inferno.Tenants.Entities;

namespace Inferno.Localization.Entities
{
    public class LocalizableString : TenantEntity<Guid>
    {
        public string CultureCode { get; set; }

        public string TextKey { get; set; }

        public string TextValue { get; set; }
    }
}