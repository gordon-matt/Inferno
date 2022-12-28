using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components
{
    public interface IContentBlockEditor
    {
        ContentBlock Data { get; set; }

        string Save();
    }
}