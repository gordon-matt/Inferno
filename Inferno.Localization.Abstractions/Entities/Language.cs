using Inferno.Tenants.Entities;

namespace Inferno.Localization.Entities
{
    public class Language : TenantEntity<Guid>
    {
        public string Name { get; set; }

        public string CultureCode { get; set; }

        public bool IsRTL { get; set; }

        public bool IsEnabled { get; set; }

        public int SortOrder { get; set; }
    }
}