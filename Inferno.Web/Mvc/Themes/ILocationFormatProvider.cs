namespace Inferno.Web.Mvc.Themes;

/// <summary>
/// Razor location formats for one assembly. <see cref="Razor.InfernoViewLocationExpander"/> appends the formats
/// whose provider lives in the controller's assembly.
/// </summary>
public interface ILocationFormatProvider
{
    /// <summary>Location formats for area views.</summary>
    IEnumerable<string> AreaViewLocationFormats { get; }

    /// <summary>Location formats for area layouts.</summary>
    IEnumerable<string> AreaMasterLocationFormats { get; }

    /// <summary>Location formats for area partials.</summary>
    IEnumerable<string> AreaPartialViewLocationFormats { get; }

    /// <summary>Location formats for views.</summary>
    IEnumerable<string> ViewLocationFormats { get; }

    /// <summary>Location formats for layouts.</summary>
    IEnumerable<string> MasterLocationFormats { get; }

    /// <summary>Location formats for partials.</summary>
    IEnumerable<string> PartialViewLocationFormats { get; }
}