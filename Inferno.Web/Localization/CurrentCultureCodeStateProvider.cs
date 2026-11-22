using Microsoft.AspNetCore.Http;

namespace Inferno.Web.Localization;

/// <summary>Supplies <see cref="InfernoWebConstants.StateProviders.CurrentCultureCode"/> from <see cref="IWebCultureManager"/>.</summary>
public class CurrentCultureCodeStateProvider : IWorkContextStateProvider
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IWebCultureManager cultureManager;

    /// <summary>Creates the provider.</summary>
    /// <param name="httpContextAccessor">The request passed to the culture manager.</param>
    /// <param name="cultureManager">Resolves the culture code.</param>
    public CurrentCultureCodeStateProvider(
        IHttpContextAccessor httpContextAccessor,
        IWebCultureManager cultureManager)
    {
        this.cultureManager = cultureManager;
        this.httpContextAccessor = httpContextAccessor;
    }

    #region IWorkContextStateProvider Members

    /// <inheritdoc />
    public Func<IWorkContext, T> Get<T>(string name) => name == InfernoWebConstants.StateProviders.CurrentCultureCode
        ? (ctx => (T)(object)cultureManager.GetCurrentCulture(httpContextAccessor.HttpContext))
        : null;

    #endregion IWorkContextStateProvider Members
}