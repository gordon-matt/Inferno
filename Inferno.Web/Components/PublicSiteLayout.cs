using Inferno.Web.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Inferno.Web.Components;

/// <summary>
/// Layout stand-in for a page that lives in a library and should wear the host's public layout.
/// A library cannot reference the host project, so the host names its layout in <see cref="InfernoWebOptions.PublicLayout"/>.
/// </summary>
public class PublicSiteLayout : LayoutComponentBase
{
    [Inject]
    private InfernoWebOptions Options { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (Options.PublicLayout is null)
        {
            builder.AddContent(0, Body);
            return;
        }

        builder.OpenComponent<LayoutView>(1);
        builder.AddAttribute(2, nameof(LayoutView.Layout), Options.PublicLayout);
        builder.AddAttribute(3, nameof(LayoutView.ChildContent), Body);
        builder.CloseComponent();
    }
}
