using Dependo;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;

/// <summary>Supplies the blocks to render for a named zone.</summary>
public interface IContentBlockProvider
{
    /// <summary>Loads the blocks for a zone.</summary>
    /// <param name="zoneName">The zone name.</param>
    /// <returns>The blocks to render.</returns>
    IEnumerable<IContentBlock> GetContentBlocks(string zoneName);
}

/// <summary>Loads the current page's enabled blocks for a zone, using the current culture.</summary>
public class DefaultContentBlockProvider : IContentBlockProvider
{
    private readonly IContentBlockService contentBlockService;

    /// <summary>Stores the content-block service.</summary>
    /// <param name="contentBlockService">Loads blocks by zone, culture, and page.</param>
    public DefaultContentBlockProvider(IContentBlockService contentBlockService)
    {
        this.contentBlockService = contentBlockService;
    }

    /// <summary>
    /// Loads enabled blocks for <paramref name="zoneName"/> in the current culture.
    /// Includes the current page's blocks when a page id is in the work context.
    /// </summary>
    /// <param name="zoneName">The zone name. A missing zone is created and then yields no blocks.</param>
    /// <returns>The enabled blocks. A null block is omitted.</returns>
    public virtual IEnumerable<IContentBlock> GetContentBlocks(string zoneName)
    {
        var workContext = DependoResolver.Instance.Resolve<IWorkContext>();
        var pageId = workContext.GetState<Guid?>("CurrentPageId");

        var contentBlocks = contentBlockService.GetContentBlocks(zoneName, workContext.CurrentCultureCode, pageId: pageId);
        return contentBlocks.Where(IsVisible).ToList();
    }

    protected static bool IsVisible(IContentBlock contentBlock) => contentBlock != null && contentBlock.Enabled;
}