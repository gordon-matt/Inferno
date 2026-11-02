using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using PageKrafter.Blocks;
using PageKrafter.Documents;
using PageKrafter.Rendering;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>
/// A region whose contents are the content blocks assigned to a zone. The saved page contains a
/// <c>[[ContentZone:Name]]</c> token, which the page view replaces with those blocks.
/// </summary>
public sealed class PageContentZoneBlock(IZoneService zones) : BlockDefinition
{
    public const string TypeName = "cms.content-zone";

    public override string Type => TypeName;

    public override string Label => "Content zone";

    public override string Category => "Content";

    public override string Icon => BlockIcons.Zone;

    public override string Description => "Shows the content blocks assigned to a zone.";

    public override bool UsesCommonStyles => false;

    public override IReadOnlyList<BlockField> Fields { get; } =
    [
        new BlockField
        {
            Name = "zone",
            Label = "Zone",
            Editor = FieldEditors.Select,
            HasDynamicOptions = true,
            HelpText = "Add and arrange the zone's content blocks on the Content Blocks page.",
        },
    ];

    public override Task<IReadOnlyList<FieldOption>> GetOptionsAsync(BlockField field, BlockNode node, CancellationToken cancellationToken = default)
    {
        using var connection = zones.OpenConnection();
        var names = connection.Query()
            .OrderBy(zone => zone.Name)
            .Select(zone => zone.Name)
            .ToList();

        IReadOnlyList<FieldOption> options = [new FieldOption(string.Empty, "Choose a zone…"), .. names.Select(name => new FieldOption(name, name))];
        return Task.FromResult(options);
    }

    public override string Summarize(BlockNode node) => node.Get("zone") ?? string.Empty;

    public override ValueTask<string> RenderAsync(BlockRenderContext context)
    {
        string zone = context.Value("zone").Replace("]]", string.Empty, StringComparison.Ordinal).Trim();
        if (!context.IsEditor)
        {
            return ValueTask.FromResult(zone.Length == 0 ? string.Empty : $"[[ContentZone:{zone}]]");
        }

        if (zone.Length == 0)
        {
            return ValueTask.FromResult("""<div class="pk-missing">Choose a zone for this block in the settings.</div>""");
        }

        return ValueTask.FromResult($"""
            <div style="padding:28px 12px 12px;border:1px dashed #8b5cf6;border-radius:6px;background:#faf5ff;color:#6d28d9;font:600 13px system-ui,sans-serif">
            Zone: {Html.Encode(zone)}
            <span style="display:block;margin-top:4px;font-weight:400;color:#6b7280">Its content blocks are shown when the page is viewed.</span>
            </div>
            """);
    }
}
