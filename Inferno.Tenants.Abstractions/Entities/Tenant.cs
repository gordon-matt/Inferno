using Extenso.Data.Entity;

namespace Inferno.Tenants.Entities;

public class Tenant : BaseEntity<int>
{
    public string Name { get; set; }

    public string Url { get; set; }

    public bool SslEnabled { get; set; }

    public string SecureUrl { get; set; }

    public string Hosts { get; set; }
}