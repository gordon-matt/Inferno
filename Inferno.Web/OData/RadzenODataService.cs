using Dependo;
using Inferno.Web.Identity;
using Inferno.Web.Models;
using Microsoft.Extensions.Configuration;
using Radzen;
using System.Net.Http.Headers;
using System.Text;

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

        private ITokenService TokenService { get; init; }
        private IWorkContext WorkContext { get; init; }
        private IConfiguration Configuration { get; init; }

        public RadzenODataService(string entitySetName)
        {
            TokenService = EngineContext.Current.Resolve<ITokenService>();
            WorkContext = EngineContext.Current.Resolve<IWorkContext>();
            Configuration = EngineContext.Current.Resolve<IConfiguration>();
            baseUri = new Uri(Configuration.GetValue<string>("ApiBaseUri"));

            var httpClientFactory = EngineContext.Current.Resolve<IHttpClientFactory>();
            httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.entitySetName = entitySetName;
        }

        // TODO: Store the token to reuse
        private async Task<string> GetBearerTokenAsync()
        {
            return await TokenService.GenerateJsonWebTokenAsync(WorkContext.CurrentUser.Id);
        }

        public virtual async Task<ApiResponse<ODataServiceResult<TEntity>>> FindAsync(
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

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse<ODataServiceResult<TEntity>>.Failure(reason);
            }

            var data = await response.ReadAsync<ODataServiceResult<TEntity>>();
            return ApiResponse<ODataServiceResult<TEntity>>.Success(data);
        }

        public virtual async Task<ApiResponse<TEntity>> FindOneAsync(TKey key)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await httpClient.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse<TEntity>.Failure(reason);
            }

            var data = await response.ReadAsync<TEntity>();
            return ApiResponse<TEntity>.Success(data);
        }

        public virtual async Task<ApiResponse<TEntity>> InsertAsync(TEntity entity)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, entitySetName);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(ODataJsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
            };
            using var response = await httpClient.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse<TEntity>.Failure(reason);
            }

            var data = await response.ReadAsync<TEntity>();
            return ApiResponse<TEntity>.Success(data);
        }

        public virtual async Task<ApiResponse<TEntity>> UpdateAsync(TKey key, TEntity entity)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(ODataJsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
            };
            using var response = await httpClient.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse<TEntity>.Failure(reason);
            }

            var data = await response.ReadAsync<TEntity>();
            return ApiResponse<TEntity>.Success(data);
        }

        public virtual async Task<ApiResponse> DeleteAsync(TKey key)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync());

            var uri = new Uri(baseUri, $"{entitySetName}({key})");
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            using var response = await httpClient.SendAsync(httpRequestMessage);

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse.Failure(reason);
            }

            return ApiResponse.Success();
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