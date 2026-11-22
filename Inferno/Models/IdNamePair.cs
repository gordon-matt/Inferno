namespace Inferno.Models;

/// <summary>An identifier paired with a display name.</summary>
/// <typeparam name="TId">The type of <see cref="Id"/>.</typeparam>
public class IdNamePair<TId>
{
    /// <summary>The identifier.</summary>
    public TId Id { get; set; }

    /// <summary>The display name. May be <see langword="null"/>.</summary>
    public string Name { get; set; }
}
