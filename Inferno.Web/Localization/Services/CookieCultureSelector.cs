using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Localization.Services;

/// <summary>Reads the <c>CurrentCulture</c> cookie. An empty cookie is a choice of the invariant culture.</summary>
public class CookieCultureSelector : ICultureSelector
{
    /// <inheritdoc />
    public CultureSelectorResult GetCulture(HttpContext context)
    {
        string cookie = context.Request.Cookies["CurrentCulture"];
        if (cookie != null)
        {
            // we allow null or empty value (so we can work with invariant culture for editing pages, etc)
            return new CultureSelectorResult { Priority = -4, CultureCode = cookie };
        }
        return null;
    }
}