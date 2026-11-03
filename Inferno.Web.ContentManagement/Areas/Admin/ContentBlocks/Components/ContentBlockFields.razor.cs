using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

/// <summary>The fields every block has, shared by the site-wide and entity-type block screens.</summary>
public partial class ContentBlockFields
{
    /// <summary>Edited in place; the page owns this copy and saves it.</summary>
    [Parameter, EditorRequired]
    public ContentBlock Block { get; set; }

    /// <summary>The block type can only be chosen before the block exists, because its settings depend on it.</summary>
    [Parameter]
    public bool IsNew { get; set; }

    [Parameter]
    public IReadOnlyList<BlockTypeOption> BlockTypes { get; set; } = [];

    [Parameter]
    public IReadOnlyList<ZoneOption> Zones { get; set; } = [];

    [Parameter]
    public IReadOnlyDictionary<string, string> Errors { get; set; }
}
