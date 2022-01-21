using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Localization.Services
{
    public interface ICultureSelector
    {
        CultureSelectorResult GetCulture(HttpContext context);
    }
}