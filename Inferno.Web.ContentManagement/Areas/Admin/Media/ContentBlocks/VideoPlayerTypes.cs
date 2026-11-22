namespace Inferno.Web.ContentManagement.Areas.Admin.Media.ContentBlocks;

/// <summary>
/// DRM protection values previously defined by Blazorise.Video.
/// Numeric values are kept so existing content-block JSON still deserializes.
/// </summary>
public enum VideoProtectionType
{
    /// <summary>Clear Key. Stored as 0.</summary>
    ClearKey = 0,

    /// <summary>PlayReady. Stored as 1.</summary>
    PlayReady = 1,

    /// <summary>Widevine. Stored as 2.</summary>
    Widevine = 2
}

/// <summary>
/// Streaming library values previously defined by Blazorise.Video.
/// </summary>
public enum StreamingLibrary
{
    /// <summary>MPEG-DASH. Stored as 0.</summary>
    Dash = 0,

    /// <summary>HTTP Live Streaming. Stored as 1.</summary>
    Hls = 1
}

/// <summary>
/// Plyr control identifiers previously defined by Blazorise.Video.
/// Stored with a video block; the HTML5 player does not render a custom control set.
/// </summary>
public static class VideoControlsType
{
    /// <summary>The large play button.</summary>
    public const string PlayLarge = "play-large";

    /// <summary>The restart control.</summary>
    public const string Restart = "restart";

    /// <summary>The rewind control.</summary>
    public const string Rewind = "rewind";

    /// <summary>The play and pause control.</summary>
    public const string Play = "play";

    /// <summary>The fast-forward control.</summary>
    public const string FastForward = "fast-forward";

    /// <summary>The progress bar.</summary>
    public const string Progress = "progress";

    /// <summary>The current-time label.</summary>
    public const string CurrentTime = "current-time";

    /// <summary>The duration label.</summary>
    public const string Duration = "duration";

    /// <summary>The mute control.</summary>
    public const string Mute = "mute";

    /// <summary>The volume control.</summary>
    public const string Volume = "volume";

    /// <summary>The captions control.</summary>
    public const string Captions = "captions";

    /// <summary>The settings control.</summary>
    public const string Settings = "settings";

    /// <summary>The picture-in-picture control.</summary>
    public const string Pip = "pip";

    /// <summary>The AirPlay control.</summary>
    public const string Airplay = "airplay";

    /// <summary>The download control.</summary>
    public const string Download = "download";

    /// <summary>The fullscreen control.</summary>
    public const string Fullscreen = "fullscreen";
}
