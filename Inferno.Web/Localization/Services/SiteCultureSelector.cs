//using Inferno.Infrastructure;
//using Microsoft.AspNetCore.Http;

//namespace Inferno.Web.Localization.Services
//{
//    public class SiteCultureSelector : ICultureSelector
//    {
//        public CultureSelectorResult GetCulture(HttpContext context)
//        {
//            string cultureCode = EngineContext.Current.Resolve<InfernoSiteSettings>().DefaultLanguage;
//            return string.IsNullOrEmpty(cultureCode)
//                ? null
//                : new CultureSelectorResult { Priority = -5, CultureCode = cultureCode };
//        }
//    }
//}