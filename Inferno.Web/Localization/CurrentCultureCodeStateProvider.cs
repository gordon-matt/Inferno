using Inferno.Caching;
using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Localization;

public class CurrentCultureCodeStateProvider : IWorkContextStateProvider
{
    private readonly ICacheManager cacheManager;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IWebCultureManager cultureManager;

    public CurrentCultureCodeStateProvider(
        ICacheManager cacheManager,
        IHttpContextAccessor httpContextAccessor,
        IWebCultureManager cultureManager)
    {
        this.cacheManager = cacheManager;
        this.cultureManager = cultureManager;
        this.httpContextAccessor = httpContextAccessor;
    }

    #region IWorkContextStateProvider Members

    public Func<IWorkContext, T> Get<T>(string name) => name == InfernoWebConstants.StateProviders.CurrentCultureCode
            ? (ctx => cacheManager.Get(InfernoWebConstants.CacheKeys.CurrentCulture, () =>
            {
                return (T)(object)cultureManager.GetCurrentCulture(httpContextAccessor.HttpContext);
            }))
            : null;

    #endregion IWorkContextStateProvider Members
}