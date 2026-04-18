using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Dependo;
using Inferno.Web.Identity;
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
        // How long before the real token expiry to proactively refresh.
        private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(5);

        // Tokens must be cached per user because this service is often registered as a
        // singleton. A single shared cached token would leak the first user's identity
        // to every subsequent request from other users.
        private static readonly ConcurrentDictionary<string, CachedToken> tokenCache = new();

        protected readonly Uri baseUri;
        protected readonly string entitySetName;
        protected readonly HttpClient httpClient;
        private bool isDisposed;

        private ITokenService TokenService { get; init; }
        private IWorkContext WorkContext { get; init; }
        private IConfiguration Configuration { get; init; }

        protected RadzenODataService(string entitySetName)
        {
            TokenService = DependoResolver.Instance.Resolve<ITokenService>();
            WorkContext = DependoResolver.Instance.Resolve<IWorkContext>();
            Configuration = DependoResolver.Instance.Resolve<IConfiguration>();
            baseUri = new Uri(Configuration.GetValue<string>("ApiBaseUri"));

            var httpClientFactory = DependoResolver.Instance.Resolve<IHttpClientFactory>();
            httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.entitySetName = entitySetName;
        }

        private async Task<string> GetBearerTokenAsync(bool forceRefresh = false)
        {
            var currentUser = WorkContext.CurrentUser
                ?? throw new InvalidOperationException("No authenticated user found. Please ensure you are logged in.");

            string userId = currentUser.Id;

            if (!forceRefresh
                && tokenCache.TryGetValue(userId, out var cached)
                && DateTime.UtcNow < cached.ExpiresAtUtc - TokenRefreshBuffer)
            {
                return cached.Token;
            }

            string token = await TokenService.GenerateJsonWebTokenAsync(userId).ConfigureAwait(false);

            // Tokens are currently issued with a 120 minute lifetime by TokenService.
            // We cache slightly shorter than that to avoid serving a token that will
            // expire mid-request.
            tokenCache[userId] = new CachedToken(token, DateTime.UtcNow.AddMinutes(115));

            return token;
        }

        private void InvalidateCurrentUserToken()
        {
            string userId = WorkContext.CurrentUser?.Id;
            if (!string.IsNullOrEmpty(userId))
            {
                tokenCache.TryRemove(userId, out _);
            }
        }

        /// <summary>
        /// Sends a request with a bearer token. Automatically retries once with a
        /// freshly-issued token if the server rejects the token as unauthorized.
        /// </summary>
        private async Task<HttpResponseMessage> SendAuthorizedAsync(Func<HttpRequestMessage> requestFactory)
        {
            var request = requestFactory();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync().ConfigureAwait(false));

            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                response.Dispose();
                request.Dispose();
                InvalidateCurrentUserToken();

                var retry = requestFactory();
                retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetBearerTokenAsync(forceRefresh: true).ConfigureAwait(false));
                response = await httpClient.SendAsync(retry).ConfigureAwait(false);
                retry.Dispose();
            }
            else
            {
                request.Dispose();
            }

            return response;
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
            var uri = new Uri(baseUri, entitySetName)
                .GetODataUri(filter: filter, top: top, skip: skip, orderby: orderby, expand: expand, select: select, count: count);

            using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Get, uri));

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse<ODataServiceResult<TEntity>>.Failure($"HTTP {(int)response.StatusCode}: {reason}");
            }

            var data = await response.ReadAsync<ODataServiceResult<TEntity>>();
            return ApiResponse<ODataServiceResult<TEntity>>.Success(data);
        }

        public virtual async Task<ApiResponse<TEntity>> FindOneAsync(TKey key)
        {
            var uri = new Uri(baseUri, $"{entitySetName}({key})");

            using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Get, uri));

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse<TEntity>.Failure($"HTTP {(int)response.StatusCode}: {reason}");
            }

            var data = await response.ReadAsync<TEntity>();
            return ApiResponse<TEntity>.Success(data);
        }

        public virtual async Task<ApiResponse<TEntity>> InsertAsync(TEntity entity)
        {
            var uri = new Uri(baseUri, entitySetName);

            using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(ODataJsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
            });

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse<TEntity>.Failure($"HTTP {(int)response.StatusCode}: {reason}");
            }

            var data = await response.ReadAsync<TEntity>();
            return ApiResponse<TEntity>.Success(data);
        }

        public virtual async Task<ApiResponse<TEntity>> UpdateAsync(TKey key, TEntity entity)
        {
            var uri = new Uri(baseUri, $"{entitySetName}({key})");

            using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(ODataJsonSerializer.Serialize(entity), Encoding.UTF8, "application/json")
            });

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse<TEntity>.Failure($"HTTP {(int)response.StatusCode}: {reason}");
            }

            var data = await response.ReadAsync<TEntity>();
            return ApiResponse<TEntity>.Success(data);
        }

        public virtual async Task<ApiResponse> DeleteAsync(TKey key)
        {
            var uri = new Uri(baseUri, $"{entitySetName}({key})");

            using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Delete, uri));

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse.Failure($"HTTP {(int)response.StatusCode}: {reason}");
            }

            return ApiResponse.Success();
        }

        private sealed record CachedToken(string Token, DateTime ExpiresAtUtc);

        #region IDisposable Members

        public void Dispose()
        {
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

                isDisposed = true;
            }
        }

        #endregion IDisposable Members
    }
}
