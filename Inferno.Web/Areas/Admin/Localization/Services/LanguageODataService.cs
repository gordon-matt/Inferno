using System.Text;
using Inferno.Localization.Entities;
using Inferno.Web.OData;

namespace Inferno.Web.Areas.Admin.Localization.Services
{
    public class LanguageODataService : RadzenODataService<Language, Guid>
    {
        public LanguageODataService()
            : base($"{InfernoWebConstants.ODataRoutes.Prefix}/{InfernoWebConstants.ODataRoutes.EntitySetNames.Language}")
        {
        }

        public virtual async Task<bool> ResetLocalizableStringsAsync()
        {
            var uri = new Uri(baseUri, $"{entitySetName}/Default.ResetLocalizableStrings");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(null, Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(httpRequestMessage);
            return response.IsSuccessStatusCode;
        }
    }
}