using System.Net;
using System.Text;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Models;
using Inferno.Web.Models;
using Inferno.Web.OData;
using Radzen;

namespace Inferno.Web.ContentManagement.Areas.Admin.Sitemap.Services;

public interface ISitemapODataService : IRadzenODataService<SitemapConfig, int>
{
    Task<ApiResponse<ODataServiceResult<SitemapConfigModel>>> GetConfigAsync();

    Task<ApiResponse> SetConfigAsync(int id, ChangeFrequency changeFrequency, float priority);

    Task<ApiResponse> GenerateAsync();
}

public class SitemapODataService : RadzenODataService<SitemapConfig, int>, ISitemapODataService
{
    public SitemapODataService()
        : base($"{CmsConstants.ODataRoutes.Prefix}/{CmsConstants.ODataRoutes.EntitySetNames.XmlSitemap}")
    {
    }

    public async Task<ApiResponse<ODataServiceResult<SitemapConfigModel>>> GetConfigAsync()
    {
        var uri = new Uri(baseUri, $"{entitySetName}/GetConfig");

        using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Get, uri));

        if (!response.IsSuccessStatusCode)
        {
            string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ApiResponse<ODataServiceResult<SitemapConfigModel>>.Failure($"HTTP {(int)response.StatusCode}: {reason}");
        }

        var data = await response.ReadAsync<ODataServiceResult<SitemapConfigModel>>();
        return ApiResponse<ODataServiceResult<SitemapConfigModel>>.Success(data);
    }

    public async Task<ApiResponse> SetConfigAsync(int id, ChangeFrequency changeFrequency, float priority)
    {
        var uri = new Uri(baseUri, $"{entitySetName}/SetConfig");
        string body = ODataJsonSerializer.Serialize(new
        {
            id,
            changeFrequency = (byte)changeFrequency,
            priority
        });

        using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });

        if (!response.IsSuccessStatusCode)
        {
            string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ApiResponse.Failure($"HTTP {(int)response.StatusCode}: {reason}");
        }

        return ApiResponse.Success();
    }

    public async Task<ApiResponse> GenerateAsync()
    {
        var uri = new Uri(baseUri, $"{entitySetName}/Generate");

        using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });

        if (response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NoContent)
        {
            return ApiResponse.Success();
        }

        string reason = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return ApiResponse.Failure($"HTTP {(int)response.StatusCode}: {reason}");
    }
}