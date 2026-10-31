using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Localization;

public class CurrentCultureCodeStateProvider : IWorkContextStateProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IWebCultureManager cultureManager;

    public CurrentCultureCodeStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IWebCultureManager cultureManager)
    {
        this.cultureManager = cultureManager;
        this.httpContextAccessor = httpContextAccessor;
    }

    #region IWorkContextStateProvider Members

    public Func<IWorkContext, T> Get<T>(string name) => name == InfernoWebConstants.StateProviders.CurrentCultureCode
        ? (ctx => (T)(object)cultureManager.GetCurrentCulture(httpContextAccessor.HttpContext))
        : null;

    #endregion IWorkContextStateProvider Members
}