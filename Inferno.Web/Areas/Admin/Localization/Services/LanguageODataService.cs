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
            using var response = await SendAuthorizedAsync(() => new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            });
            return response.IsSuccessStatusCode;
        }
    }
}