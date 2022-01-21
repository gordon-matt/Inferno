using Extenso.Data.Entity;

namespace Inferno.Security.Membership
{
    public class InfernoPermission : BaseEntity<string>
    {
        public int? TenantId { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }
    }
}