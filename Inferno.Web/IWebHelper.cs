using Inferno.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Inferno.Web;

/// <summary>Paths, the current request, and restarting the host.</summary>
public partial interface IWebHelper
{
    /// <summary>The content root of the host.</summary>
    string ContentRootPath { get; }

    /// <summary>The web root path.</summary>
    string WebRootPath { get; }

    /// <summary>Whether the current request is HTTPS.</summary>
    /// <returns><see langword="true"/> when a request is available and it is HTTPS; otherwise <see langword="false"/>.</returns>
    bool IsCurrentConnectionSecured();

    /// <summary>The remote IP of the current connection.</summary>
    /// <returns>The address text.</returns>
    string GetRemoteIpAddress();

    /// <summary>
    /// The origin of the current request, with a trailing slash.
    /// Empty when no request is available or the host header is missing.
    /// </summary>
    /// <param name="useSsl"><see langword="null"/> follows the current connection. Otherwise forces HTTP or HTTPS.</param>
    /// <returns>The origin, or an empty string.</returns>
    string GetUrlHost(bool? useSsl = null);

    /// <summary>The Referer header of the current request.</summary>
    /// <returns>The header value.</returns>
    string GetUrlReferrer();

    /// <summary>Stops the application so the host process can start it again.</summary>
    /// <exception cref="InfernoException">The application could not be stopped.</exception>
    void RestartAppDomain();
}

/// <summary>Reads paths and the current request from the host environment.</summary>
public partial class WebHelper : IWebHelper
{
    private readonly IHostApplicationLifetime applicationLifetime;
    private readonly IWebHostEnvironment webHostEnvironment;
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <inheritdoc />
    public string ContentRootPath => webHostEnvironment.ContentRootPath;

    /// <inheritdoc />
    public string WebRootPath => webHostEnvironment.WebRootPath;

    /// <summary>Creates the helper from the host lifetime, environment, and current HTTP context.</summary>
    /// <param name="applicationLifetime">Stopped by <see cref="IWebHelper.RestartAppDomain"/>.</param>
    /// <param name="webHostEnvironment">Supplies the content and web roots.</param>
    /// <param name="httpContextAccessor">The current request.</param>
    public WebHelper(
        IHostApplicationLifetime applicationLifetime,
        IWebHostEnvironment webHostEnvironment,
        IHttpContextAccessor httpContextAccessor)
    {
        this.applicationLifetime = applicationLifetime;
        this.webHostEnvironment = webHostEnvironment;
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public virtual bool IsCurrentConnectionSecured() => IsRequestAvailable() && httpContextAccessor.HttpContext.Request.IsHttps;

    /// <inheritdoc />
    public virtual string GetRemoteIpAddress() => httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

    /// <inheritdoc />
    public virtual string GetUrlHost(bool? useSsl = null)
    {
        if (!IsRequestAvailable())
        {
            return string.Empty;
        }

        var hostHeader = httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Host];

        if (!hostHeader.HasValue || StringValues.IsNullOrEmpty(hostHeader.Value))
        {
            return string.Empty;
        }

        if (!useSsl.HasValue)
        {
            useSsl = IsCurrentConnectionSecured();
        }

        string host = $"{(useSsl.Value ? Uri.UriSchemeHttps : Uri.UriSchemeHttp)}{Uri.SchemeDelimiter}{hostHeader.Value.FirstOrDefault()}";

        // Ensure that host ends with a slash
        host = $"{host.TrimEnd('/')}/";

        return host;
    }

    /// <inheritdoc />
    public virtual string GetUrlReferrer() => httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer];

    /// <inheritdoc />
    public virtual void RestartAppDomain()
    {
        try
        {
            // TODO: Test in IIS
            applicationLifetime.StopApplication();
        }
        catch
        {
            throw new InfernoException(
                $"This site needs to be restarted, but was unable to do so.{Environment.NewLine}Please restart it manually for changes to take effect.");
        }
    }

    /// <summary>Whether an HTTP request can be read from the accessor.</summary>
    /// <returns><see langword="false"/> when the context or request is missing or cannot be read.</returns>
    protected virtual bool IsRequestAvailable()
    {
        if (httpContextAccessor?.HttpContext == null)
        {
            return false;
        }

        try
        {
            if (httpContextAccessor.HttpContext.Request == null)
            {
                return false;
            }
        }
        catch
        {
            return false;
        }

        return true;
    }
}