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

    IReadOnlyList<WidgetDefinition> Widgets { get; }

    void AddDefaults(DashboardBoard board);
}
