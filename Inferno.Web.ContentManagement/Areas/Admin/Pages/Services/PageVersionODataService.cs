using System.Text;
using System.Text.Json;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;
using Inferno.Web.Models;
using Inferno.Web.OData;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services;

public interface IPageVersionODataService : IRadzenODataService<PageVersion, Guid>
{
    Task<ApiResponse> RestoreVersionAsync(Guid versionId);
}

public class PageVersionODataService : RadzenODataService<PageVersion, Guid>, IPageVersionODataService
{
    public PageVersionODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.PageVersion}")
    {
    }

    public async Task<ApiResponse> RestoreVersionAsync(Guid versionId)
    {
        var uri = new Uri(baseUri, $"{entitySetName}({versionId})/Default.RestoreVersion");
        using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { }), Encoding.UTF8, "application/json")
        });

        if (!response.IsSuccessStatusCode)
        {
            string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ApiResponse.Failure($"HTTP {(int)response.StatusCode}: {reason}");
        }

        return ApiResponse.Success();
    }
}