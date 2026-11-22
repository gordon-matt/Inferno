using Extenso.Data.Entity;

namespace Inferno.Security.Membership;

/// <summary>A role as the membership service returns it. Several queries leave <see cref="TenantId"/> unset.</summary>
public class InfernoRole : BaseEntity<string>
{
    /// <summary>The tenant this role belongs to. <see langword="null"/> is a host role. Role queries that select by id or name do not copy this value.</summary>
    public int? TenantId { get; set; }

    /// <summary>The role name.</summary>
    public string Name { get; set; }
}
