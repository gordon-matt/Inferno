using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components
{
    public interface IContentBlockDisplay
    {
        ContentBlock Data { get; set; }
    }
}