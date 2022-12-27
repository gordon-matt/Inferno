using Dependo;
using Dependo.Autofac;
using Extenso.Data.Entity;
using Inferno.Caching;
using Inferno.Data.Services;
using Inferno.Web.ContentManagement.Areas.Admin.Pages.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages.Services
{
    public interface IPageTypeService : IGenericDataService<PageType>
    {
        InfernoPageType GetInfernoPageType(string name);

        IEnumerable<InfernoPageType> GetInfernoPageTypes();
    }

    public class PageTypeService : GenericDataService<PageType>, IPageTypeService
    {
        private static Lazy<IEnumerable<InfernoPageType>> infernoPageTypes;

        public PageTypeService(ICacheManager cacheManager, IRepository<PageType> repository)
            : base(cacheManager, repository)
        {
            infernoPageTypes = new Lazy<IEnumerable<InfernoPageType>>(() =>
            {
                var typeFinder = EngineContext.Current.Resolve<ITypeFinder>();

                var pageTypes = typeFinder.FindClassesOfType<InfernoPageType>()
                    .Select(x => (InfernoPageType)Activator.CreateInstance(x));

                return pageTypes.Where(x => x.IsEnabled);
            });
        }

        #region IPageTypeService Members

        public InfernoPageType GetInfernoPageType(string name) => infernoPageTypes.Value.FirstOrDefault(x => x.Name == name);

        public IEnumerable<InfernoPageType> GetInfernoPageTypes() => infernoPageTypes.Value;

        #endregion IPageTypeService Members
    }
}