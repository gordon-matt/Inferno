using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Components;

/// <summary>Which record an entity-type block belongs to.</summary>
public partial class EntityTargetFields
{
    [Parameter]
    public string EntityType { get; set; }

    [Parameter]
    public EventCallback<string> EntityTypeChanged { get; set; }

    [Parameter]
    public string EntityId { get; set; }

    [Parameter]
    public EventCallback<string> EntityIdChanged { get; set; }

    [Parameter]
    public IReadOnlyDictionary<string, string> Errors { get; set; }
}
