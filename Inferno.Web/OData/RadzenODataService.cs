using System.Net.Http.Headers;
using System.Text;
using Dependo;
using Extenso;
using Inferno.Web.Models;
using Microsoft.Extensions.Configuration;
using Radzen;

namespace Inferno.Web.OData
{
    public abstract class RadzenODataService<TEntity> : RadzenODataService<TEntity, int>
        where TEntity : class
    {
        protected RadzenODataService(string entitySetName)
            : base(entitySetName)
        {
        }
    }

    public abstract class RadzenODataService<TEntity, TKey> : IRadzenODataService<TEntity, TKey>, IDisposable
        where TEntity : class
    {
        protected readonly Uri baseUri;
        protected readonly string entitySetName;
        protected readonly HttpClient httpClient;
        private bool isDisposed;

        private IConfiguration Configuration { get; init; }

        public RadzenODataService(string entitySetName)
        {
            Configuration = EngineContext.Current.Resolve<IConfiguration>();
            baseUri = new Uri(Configuration.GetValue<string>("ApiBaseUri"));

            var httpClientFactory = EngineContext.Current.Resolve<IHttpClientFactory>();
            httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.entitySetName = entitySetName;
        }

        private async Task<string> GetBearerTokenAsync()
        {
            string json = new AuthModel { ApiKey = Configuration.GetValue<string>("ApiKey") }.JsonSerialize();
            using var data = new StringContent(json, Encoding.UTF8, "application/json");
            using var responseMessage = await httpClient.PostAsync(Configuration.GetValue<string>("ApiAuthUri"), data);
            if (responseMessage.IsSuccessStatusCode)
            {
                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var response = responseData.JsonDeserialize<TokenModel>();
                return response.Token;
            }

            return string.Empty;
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
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, entitySetName);
            uri = uri.GetODataUri(filter: filter, top: top, skip: skip, orderby: orderby, expand: expand, select: select, count: count);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<ODataServiceResult<TEntity>>();
        }

        public virtual async Task<TEntity> FindOneAsync(TKey key)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<TEntity>();
        }

        public virtual async Task<TEntity> InsertAsync(TEntity entity)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, entitySetName);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(ODataJsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
            };
            using var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<TEntity>();
        }

        public virtual async Task<TEntity> UpdateAsync(TKey key, TEntity entity)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(ODataJsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
            };
            using var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<TEntity>();
        }

        public virtual async Task<HttpResponseMessage> DeleteAsync(TKey key)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
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