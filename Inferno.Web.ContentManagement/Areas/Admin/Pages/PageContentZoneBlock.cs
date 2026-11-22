using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using PageKrafter.Blocks;
using PageKrafter.Documents;
using PageKrafter.Rendering;

namespace Inferno.Web.ContentManagement.Areas.Admin.Pages;

/// <summary>
/// A region whose contents are the content blocks assigned to a zone. The saved page contains a
/// <c>[[ContentZone:Name]]</c> token, which the page view replaces with those blocks.
/// </summary>
/// <param name="zones">Supplies the zone names shown in the block's zone field.</param>
public sealed class PageContentZoneBlock(IZoneService zones) : BlockDefinition
{
    /// <summary>The block type stored in a page document.</summary>
    public const string TypeName = "cms.content-zone";

    /// <summary>Returns <see cref="TypeName"/>.</summary>
    public override string Type => TypeName;

    /// <summary>The name shown in the page builder.</summary>
    public override string Label => "Content zone";

    /// <summary>The page-builder category this block is listed under.</summary>
    public override string Category => "Content";

    /// <summary>The page-builder icon.</summary>
    public override string Icon => BlockIcons.Zone;

    /// <summary>The description shown for the block.</summary>
    public override string Description => "Shows the content blocks assigned to a zone.";

    /// <summary><see langword="false"/>, so the block does not offer the shared style fields.</summary>
    public override bool UsesCommonStyles => false;

    /// <summary>The zone select field. Its choices come from <see cref="GetOptionsAsync"/>.</summary>
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

    /// <summary>Lists every zone name, with an empty choice first.</summary>
    /// <param name="field">Not read.</param>
    /// <param name="node">Not read.</param>
    /// <param name="cancellationToken">Not observed.</param>
    /// <returns>The zone choices.</returns>
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

    /// <summary>Returns the selected zone name.</summary>
    /// <param name="node">The block node.</param>
    /// <returns>The zone name, or empty when it is not set.</returns>
    public override string Summarize(BlockNode node) => node.Get("zone") ?? string.Empty;

    /// <summary>
    /// Renders a <c>[[ContentZone:Name]]</c> token for visitors, or a placeholder in the editor.
    /// </summary>
    /// <param name="context">The block being rendered. A zone value containing <c>]]</c> has that text removed.</param>
    /// <returns>The token, the editor placeholder, or empty when a visitor's block has no zone.</returns>
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
