namespace Inferno.Web.Localization.Services;

/// <summary>A culture proposed by one <see cref="ICultureSelector"/>.</summary>
public class CultureSelectorResult
{
    /// <summary>Higher values beat other selectors. The cookie selector uses -4.</summary>
    public int Priority { get; set; }

    /// <summary>The proposed culture code. Empty is allowed and means the invariant culture.</summary>
    public string CultureCode { get; set; }
}