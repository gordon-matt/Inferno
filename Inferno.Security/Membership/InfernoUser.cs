using Extenso.Data.Entity;

namespace Inferno.Security.Membership;

public class InfernoUser : BaseEntity<string>
{
    public int? TenantId { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public bool IsLockedOut { get; set; }

    /// <summary>When true, the account can sign in without a confirmation email. The installation admin uses this.</summary>
    public bool EmailConfirmed { get; set; }

    public override string ToString() => UserName;
}