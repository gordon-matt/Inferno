using System.Text;
using System.Text.Json;
using Inferno.Tasks.Entities;
using Inferno.Web.Models;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.ScheduledTasks.Services;

public class ScheduledTaskODataService : RadzenODataService<ScheduledTask, int>
{
    public ScheduledTaskODataService()
        : base($"{InfernoWebConstants.ODataRoutes.Prefix}/{InfernoWebConstants.ODataRoutes.EntitySetNames.ScheduledTask}")
    {
    }

    public virtual async Task<ApiResponse> RunNowAsync(int taskId)
    {
        var uri = new Uri(baseUri, $"{entitySetName}/Default.RunNow");
        string payload = JsonSerializer.Serialize(new { taskId });

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