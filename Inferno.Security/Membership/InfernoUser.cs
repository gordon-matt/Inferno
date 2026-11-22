using Extenso.Data.Entity;

namespace Inferno.Security.Membership;

/// <summary>A user as the membership service returns it.</summary>
public class InfernoUser : BaseEntity<string>
{
    /// <summary>The tenant this account belongs to. <see langword="null"/> is a host account.</summary>
    public int? TenantId { get; set; }

    /// <summary>The sign-in name.</summary>
    public string UserName { get; set; }

    /// <summary>The email address.</summary>
    public string Email { get; set; }

    /// <summary>
    /// Copied from Identity's lockout-enabled flag when the membership service loads or saves a user.
    /// It is not whether a lockout is currently in effect. Queries do not read it back from a lockout end date.
    /// </summary>
    public bool IsLockedOut { get; set; }

    /// <summary>When true, the account can sign in without a confirmation email. The installation admin uses this. Loaded users leave this <see langword="false"/>, because the queries do not copy it.</summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>Returns <see cref="UserName"/>.</summary>
    /// <returns>The user name.</returns>
    public override string ToString() => UserName;
}
