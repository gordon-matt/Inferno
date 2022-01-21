using Inferno.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Inferno.Web
{
    public partial interface IWebHelper
    {
        string ContentRootPath { get; }

        string WebRootPath { get; }

        bool IsCurrentConnectionSecured();

        string GetRemoteIpAddress();

        string GetUrlHost(bool? useSsl = null);

        string GetUrlReferrer();

        void RestartAppDomain();
    }

    public partial class WebHelper : IWebHelper
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IHttpContextAccessor httpContextAccessor;

        public string ContentRootPath => webHostEnvironment.ContentRootPath;

        public string WebRootPath => webHostEnvironment.WebRootPath;

        public WebHelper(
            IHostApplicationLifetime applicationLifetime,
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            this.applicationLifetime = applicationLifetime;
            this.webHostEnvironment = webHostEnvironment;
            this.httpContextAccessor = httpContextAccessor;
        }

        public virtual bool IsCurrentConnectionSecured()
        {
            if (!IsRequestAvailable())
            {
                return false;
            }

            return httpContextAccessor.HttpContext.Request.IsHttps;
        }

        public virtual string GetRemoteIpAddress()
        {
            return httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }

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

        public virtual string GetUrlReferrer()
        {
            return httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer];
        }

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
}