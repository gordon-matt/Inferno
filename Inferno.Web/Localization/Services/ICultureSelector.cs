using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Localization.Services;

/// <summary>One way to choose the culture for a request. The highest priority among selectors that return a result wins.</summary>
public interface ICultureSelector
{
    /// <summary>The culture this selector wants, or <see langword="null"/> to decline.</summary>
    /// <param name="context">The current request.</param>
    /// <returns>A culture and its priority, or <see langword="null"/>.</returns>
    CultureSelectorResult GetCulture(HttpContext context);
}