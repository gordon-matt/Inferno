using System.Net.Http.Json;
using Extenso.Data.Entity;
using Inferno.Localization.Entities;
using Inferno.Web.Areas.Admin.Localization.Models;
using Inferno.Web.OData;
using Radzen;

namespace Inferno.Web.Areas.Admin.Localization.Services
{
    public class LocalizableStringODataService : RadzenODataService<LocalizableString, Guid>
    {
        public LocalizableStringODataService()
            : base($"{InfernoWebConstants.ODataRoutes.Prefix}/{InfernoWebConstants.ODataRoutes.EntitySetNames.LocalizableString}")
        {
        }

        public virtual async Task<ODataServiceResult<ComparitiveLocalizableString>> GetComparitiveAsync(string cultureCode, LoadDataArgs args)
        {
            var uri = new Uri(baseUri, $"{entitySetName}/Default.GetComparitiveTable(cultureCode='{cultureCode}')");
            uri = uri.GetODataUri(filter: args.Filter, top: args.Top, skip: args.Skip, orderby: args.OrderBy, count: true);
            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await httpClient.SendAsync(httpRequestMessage);
            return await response.ReadAsync<ODataServiceResult<ComparitiveLocalizableString>>();
        }

        public virtual async Task<bool> PutComparitiveAsync(string cultureCode, ComparitiveLocalizableString row)
        {
            var data = new
            {
                CultureCode = cultureCode,
                Key = row.Key,
                Entity = row
            };

            var uri = new Uri(baseUri, $"{entitySetName}/Default.PutComparitive");
            using var response = await httpClient.PostAsJsonAsync(uri, data);
            return response.IsSuccessStatusCode;
        }

        public virtual async Task<bool> DeleteComparitiveAsync(string cultureCode, string key)
        {
            var data = new
            {
                CultureCode = cultureCode,
                Key = key
            };

            var uri = new Uri(baseUri, $"{entitySetName}/Default.DeleteComparitive");
            using var response = await httpClient.PostAsJsonAsync(uri, data);
            return response.IsSuccessStatusCode;
        }
    }
}