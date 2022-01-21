using Extenso.Data.Entity;

namespace Inferno.Security.Membership
{
    public class InfernoRole : BaseEntity<string>
    {
        public int? TenantId { get; set; }

        public string Name { get; set; }
    }
}