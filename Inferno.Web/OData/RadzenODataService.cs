using System.Net;
using System.Text;
using Extenso;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Radzen;

namespace Inferno.Web.OData
{
    public abstract class RadzenODataService<TEntity> : RadzenODataService<TEntity, int>
        where TEntity : class
    {
        protected RadzenODataService(string entitySetName, IHttpContextAccessor httpContextAccessor)
            : base(entitySetName, httpContextAccessor)
        {
        }
    }

    public abstract class RadzenODataService<TEntity, TKey> : IRadzenODataService<TEntity, TKey>, IDisposable
        where TEntity : class
    {
        protected readonly Uri baseUri;
        protected readonly string entitySetName;
        protected readonly HttpClient httpClient;
        private readonly HttpClientHandler httpClientHandler;
        private bool isDisposed;

        public RadzenODataService(string entitySetName, IHttpContextAccessor httpContextAccessor)
        {
            var httpRequest = httpContextAccessor.HttpContext.Request;
            baseUri = new Uri($"{httpRequest.Scheme}://{httpRequest.Host}{httpRequest.PathBase}/odata/");

            httpClientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            // This should not be used in production.. it's just to bypass certificate validation for localhost..
            if (baseUri.IsLoopback)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            }

            httpClient = new HttpClient(httpClientHandler);

            this.entitySetName = entitySetName;
        }

        public virtual async Task<ODataServiceResult<TEntity>> FindAsync(
            string filter = default,
            int? top = default,
            int? skip = default,
            string orderby = default,
            string expand = default,
            string select = default,
            bool? count = default)
        {
            var uri = new Uri(baseUri, entitySetName);
            uri = uri.GetODataUri(filter: filter, top: top, skip: skip, orderby: orderby, expand: expand, select: select, count: count);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<ODataServiceResult<TEntity>>();
        }

        public virtual async Task<TEntity> FindOneAsync(TKey key)
        {
            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<TEntity>();
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            var uri = new Uri(baseUri, entitySetName);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(ODataJsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<TEntity>();
        }

        public virtual async Task<TEntity> UpdateAsync(TKey key, TEntity entity)
        {
            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(ODataJsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<TEntity>();
        }

        public virtual async Task<HttpResponseMessage> DeleteAsync(TKey key)
        {
            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            return await httpClient.SendAsync(httpRequestMessage);
        }

        #region IDisposable Members

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    httpClient?.Dispose();
                    httpClientHandler?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                isDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GenericODataService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        #endregion IDisposable Members
    }
}