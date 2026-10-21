namespace Inferno.Web.ContentManagement.Areas.Admin.Media.ContentBlocks;

/// <summary>
/// DRM protection values previously defined by Blazorise.Video.
/// Numeric values are kept so existing content-block JSON still deserializes.
/// </summary>
public enum VideoProtectionType
{
    ClearKey = 0,
    PlayReady = 1,
    Widevine = 2
}

/// <summary>
/// Streaming library values previously defined by Blazorise.Video.
/// </summary>
public enum StreamingLibrary
{
    Dash = 0,
    Hls = 1
}

/// <summary>
/// Plyr control identifiers previously defined by Blazorise.Video.
/// Stored with a video block; the HTML5 player does not render a custom control set.
/// </summary>
public static class VideoControlsType
{
    public const string PlayLarge = "play-large";
    public const string Restart = "restart";
    public const string Rewind = "rewind";
    public const string Play = "play";
    public const string FastForward = "fast-forward";
    public const string Progress = "progress";
    public const string CurrentTime = "current-time";
    public const string Duration = "duration";
    public const string Mute = "mute";
    public const string Volume = "volume";
    public const string Captions = "captions";
    public const string Settings = "settings";
    public const string Pip = "pip";
    public const string Airplay = "airplay";
    public const string Download = "download";
    public const string Fullscreen = "fullscreen";
}
