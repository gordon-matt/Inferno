using Dependo;
using Inferno.Caching;
using Inferno.Localization.Services;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.Localization
{
    public class InfernoStringLocalizerFactory : IStringLocalizerFactory
    {
        private InfernoStringLocalizer stringLocalizer;

        public IStringLocalizer Create(Type resourceSource)
        {
            return Create();
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return Create();
        }

        protected IStringLocalizer Create()
        {
            if (stringLocalizer == null)
            {
                var cacheManager = EngineContext.Current.Resolve<ICacheManager>();
                var localizableStringService = EngineContext.Current.Resolve<ILocalizableStringService>();
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                stringLocalizer = new InfernoStringLocalizer(cacheManager, workContext, localizableStringService);
            }
            return stringLocalizer;
        }
    }
}