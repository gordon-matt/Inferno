using Extenso.Data.Entity;

namespace Inferno.Security.Membership;

/// <summary>One profile value stored for a user.</summary>
public class InfernoUserProfileEntry : BaseEntity<string>
{
    /// <summary>The user this entry belongs to.</summary>
    public string UserId { get; set; }

    /// <summary>The profile field name.</summary>
    public string Key { get; set; }

    /// <summary>The stored value. May be <see langword="null"/>.</summary>
    public string Value { get; set; }
}
