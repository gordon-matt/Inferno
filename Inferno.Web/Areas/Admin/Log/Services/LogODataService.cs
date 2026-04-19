using System.Text;
using System.Text.Json;
using Inferno.Logging.Entities;
using Inferno.Web.Models;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.Log.Services
{
    public class LogODataService : RadzenODataService<LogEntry, int>
    {
        public LogODataService()
            : base($"{InfernoWebConstants.ODataRoutes.Prefix}/{InfernoWebConstants.ODataRoutes.EntitySetNames.Log}")
        {
        }

        public virtual async Task<ApiResponse> ClearAsync()
        {
            var uri = new Uri(baseUri, $"{entitySetName}/Default.Clear");
            var payload = JsonSerializer.Serialize(new { });

            using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            });

            if (!response.IsSuccessStatusCode)
            {
                string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return ApiResponse.Failure($"HTTP {(int)response.StatusCode}: {reason}");
            }

            return ApiResponse.Success();
        }
    }
}
