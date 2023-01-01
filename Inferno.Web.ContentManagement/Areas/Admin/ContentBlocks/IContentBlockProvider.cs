using Dependo;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks
{
    public interface IContentBlockProvider
    {
        IEnumerable<IContentBlock> GetContentBlocks(string zoneName);
    }

    public class DefaultContentBlockProvider : IContentBlockProvider
    {
        private readonly IContentBlockService contentBlockService;

        public DefaultContentBlockProvider(IContentBlockService contentBlockService)
        {
            this.contentBlockService = contentBlockService;
        }

        public virtual IEnumerable<IContentBlock> GetContentBlocks(string zoneName)
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            Guid? pageId = workContext.GetState<Guid?>("CurrentPageId");

            var contentBlocks = contentBlockService.GetContentBlocks(zoneName, workContext.CurrentCultureCode, pageId: pageId);
            return contentBlocks.Where(IsVisible).ToList();
        }

        protected static bool IsVisible(IContentBlock contentBlock)
        {
            if (contentBlock == null || !contentBlock.Enabled)
            {
                return false;
            }

            return true;
        }
    }
}