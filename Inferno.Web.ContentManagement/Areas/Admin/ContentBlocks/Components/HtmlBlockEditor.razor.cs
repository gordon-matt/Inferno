using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

public partial class HtmlBlockEditor
{
    [Inject]
    private IWorkContext WorkContext { get; set; }
}
