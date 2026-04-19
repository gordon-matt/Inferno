using System.Text;
using System.Text.Json;
using Inferno.Web.Areas.Admin.Plugins.Models;
using Inferno.Web.Models;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.Plugins.Services;

public class PluginODataService : RadzenODataService<EdmPluginDescriptor, string>
{
    public PluginODataService()
        : base("inferno/web/PluginApi")
    {
    }

    public virtual Task<ApiResponse> InstallAsync(string systemName) =>
        PostActionAsync("Install", systemName);

    public virtual Task<ApiResponse> UninstallAsync(string systemName) =>
        PostActionAsync("Uninstall", systemName);

    private async Task<ApiResponse> PostActionAsync(string actionName, string systemName)
    {
        var uri = new Uri(baseUri, $"{entitySetName}/Default.{actionName}");
        string payload = JsonSerializer.Serialize(new { systemName });

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