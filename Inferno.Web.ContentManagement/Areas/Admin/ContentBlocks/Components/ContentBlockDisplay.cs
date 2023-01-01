using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components
{
    public abstract class ContentBlockDisplay<T> : ComponentBase
        where T : IContentBlock, new()
    {
        [Parameter]
        public T Model { get; set; }
    }
}