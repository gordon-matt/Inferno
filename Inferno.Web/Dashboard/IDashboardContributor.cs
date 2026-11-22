using GridKrafter.Components;
using GridKrafter.Models;

namespace Inferno.Web.Dashboard;

/// <summary>
/// Adds widget kinds, and the widgets a new dashboard starts with. Plugins register their own.
/// </summary>
public interface IDashboardContributor
{
    /// <summary>Lower numbers are asked first, so their sections come first.</summary>
    int Order { get; }

    /// <summary>The widget kinds this contributor adds.</summary>
    IReadOnlyList<WidgetDefinition> Widgets { get; }

    /// <summary>Adds the sections and items a new dashboard starts with.</summary>
    /// <param name="board">The board being filled for a user who has not saved one.</param>
    void AddDefaults(DashboardBoard board);
}
