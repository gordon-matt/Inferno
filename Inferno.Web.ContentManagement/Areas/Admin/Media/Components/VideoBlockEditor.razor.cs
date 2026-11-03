using Inferno.Models;
using Inferno.Web.ContentManagement.Areas.Admin.Media.ContentBlocks;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Media.Components;

public partial class VideoBlockEditor
{
    private static readonly IReadOnlyList<IdNamePair<string>> availableControls =
    [
        new() { Id = VideoControlsType.PlayLarge, Name = "Play (Large)" },
        new() { Id = VideoControlsType.Restart, Name = "Restart" },
        new() { Id = VideoControlsType.Rewind, Name = "Rewind" },
        new() { Id = VideoControlsType.Play, Name = "Play" },
        new() { Id = VideoControlsType.FastForward, Name = "Fast Forward" },
        new() { Id = VideoControlsType.Progress, Name = "Progress" },
        new() { Id = VideoControlsType.CurrentTime, Name = "Current Time" },
        new() { Id = VideoControlsType.Duration, Name = "Duration" },
        new() { Id = VideoControlsType.Mute, Name = "Mute" },
        new() { Id = VideoControlsType.Volume, Name = "Volume" },
        new() { Id = VideoControlsType.Captions, Name = "Captions" },
        new() { Id = VideoControlsType.Settings, Name = "Settings" },
        new() { Id = VideoControlsType.Pip, Name = "Picture-in-Picture" },
        new() { Id = VideoControlsType.Airplay, Name = "Airplay (Safari only)" },
        new() { Id = VideoControlsType.Download, Name = "Download" },
        new() { Id = VideoControlsType.Fullscreen, Name = "Fullscreen" },
    ];

    private bool IsControlSelected(string id) => Model.ControlsList?.Contains(id) == true;

    private void ToggleControl(string id, ChangeEventArgs args)
    {
        var selected = (Model.ControlsList ?? []).Where(x => x != id).ToList();
        if (args.Value is true)
        {
            selected.Add(id);
        }

        // Keep the player's documented order rather than the order of clicking.
        Model.ControlsList = availableControls.Select(x => x.Id).Where(selected.Contains).ToList();
    }
}
