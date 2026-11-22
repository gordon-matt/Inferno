using Inferno.Web.Localization.Services;
using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Localization;

/// <summary>Chooses the current culture from the registered selectors.</summary>
public interface IWebCultureManager
{
    /// <summary>
    /// The culture code from the selector with the highest priority.
    /// An empty code is returned as <see langword="null"/>. <see langword="null"/> when every selector declines.
    /// </summary>
    /// <param name="httpContext">The request passed to each <see cref="ICultureSelector"/>.</param>
    /// <returns>The chosen culture code, or <see langword="null"/>.</returns>
    string GetCurrentCulture(HttpContext httpContext);
}

/// <inheritdoc cref="IWebCultureManager" />
public class WebCultureManager : IWebCultureManager
{
    private readonly IEnumerable<ICultureSelector> cultureSelectors;

    /// <summary>Creates the manager.</summary>
    /// <param name="cultureSelectors">Selectors asked for a culture. The highest <see cref="CultureSelectorResult.Priority"/> wins.</param>
    public WebCultureManager(IEnumerable<ICultureSelector> cultureSelectors)
    {
        this.cultureSelectors = cultureSelectors;
    }

    #region IWebCultureManager Members

    /// <inheritdoc />
    public virtual string GetCurrentCulture(HttpContext httpContext)
    {
        var requestedCultures = cultureSelectors
            .Select(x => x.GetCulture(httpContext))
            .ToList()
            .Where(x => x != null)
            .OrderByDescending(x => x.Priority);

        string cultureCode = null;

        if (requestedCultures.Any())
        {
            cultureCode = requestedCultures.First().CultureCode;
        }

        if (cultureCode == string.Empty)
        {
            cultureCode = null;
        }

        return cultureCode;
    }

    #endregion IWebCultureManager Members
}