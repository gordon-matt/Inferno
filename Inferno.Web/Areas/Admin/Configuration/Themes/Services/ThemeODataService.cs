using System.Text;
using System.Text.Json;
using Inferno.Web.Areas.Admin.Configuration.Themes.Models;
using Inferno.Web.Models;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.Configuration.Themes.Services;

public class ThemeODataService : RadzenODataService<EdmThemeConfiguration, Guid>
{
    public ThemeODataService()
        : base("inferno/web/ThemeApi")
    {
    }

    public virtual async Task<ApiResponse> SetThemeAsync(string themeName)
    {
        var uri = new Uri(baseUri, $"{entitySetName}/Default.SetTheme");
        string payload = JsonSerializer.Serialize(new { themeName });

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