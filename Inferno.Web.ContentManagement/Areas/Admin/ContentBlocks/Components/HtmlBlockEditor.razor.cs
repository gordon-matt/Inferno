using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

/// <summary>Editor for an <see cref="Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.HtmlBlock"/>.</summary>
public partial class HtmlBlockEditor
{
    [Inject]
    private IWorkContext WorkContext { get; set; }
}
