namespace Inferno.Web.Configuration;

/// <summary>
/// A settings screen that is not one stored document. The settings page shows a tile that opens <see cref="Url"/>.
/// Register an implementation to add one. The destination page authorizes itself.
/// </summary>
public interface ISettingsSection
{
    /// <summary>Text on the tile.</summary>
    string Title { get; }

    /// <summary>Secondary text on the tile.</summary>
    string Description { get; }

    /// <summary>Icon name from the same set as the other settings tiles.</summary>
    string Icon { get; }

    /// <summary>Address the tile opens.</summary>
    string Url { get; }

    /// <summary>Policy required to see the tile. Empty when anyone who can open settings may see it.</summary>
    string Permission { get; }
}
