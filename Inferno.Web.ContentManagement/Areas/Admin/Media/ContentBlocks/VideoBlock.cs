using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.Media.Components;
using Blazorise.Video;

namespace Inferno.Web.ContentManagement.Areas.Admin.Media.ContentBlocks
{
    public class VideoBlock : ContentBlockBase
    {
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ControlId)]
        public string ControlId { get; set; }

        /// <summary>
        /// Gets or sets the current source for the player.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Source)]
        public string Source { get; set; }

        /// <summary>
        /// Hide video controls automatically after 2s of no mouse or focus movement, on control element blur (tab out),
        /// on playback start or entering fullscreen. As soon as the mouse is moved, a control element is focused or
        /// playback is paused, the controls reappear instantly. 
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.AutomaticallyHideControls)]
        public bool AutomaticallyHideControls { get; set; }

        /// <summary>
        /// Only allow one player playing at once.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.AutoPause)]
        public bool AutoPause { get; set; }

        /// <summary>
        /// Gets or sets the autoplay state of the player.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.AutoPlay)]
        public bool AutoPlay { get; set; }

        /// <summary>
        /// Click (or tap) of the video container will toggle play/pause. 
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ClickToPlay)]
        public bool ClickToPlay { get; set; }

        /// <summary>
        /// Gets or sets the controls visibility of the player. 
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Controls)]
        public bool Controls { get; set; }

        /// <summary>
        /// Gets or sets the list of controls that are rendered by the player. Possible list of values are contained in VideoControlsType
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ControlsList)]
        public IEnumerable<string> ControlsList { get; set; }

        /// <summary>
        /// Disable right click menu on video to help as very primitive obfuscation to prevent downloads of content.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.DisableContextMenu)]
        public bool DisableContextMenu { get; set; }

        /// <summary>
        /// Display the current time as a countdown rather than an incremental counter.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.InvertTime)]
        public bool InvertTime { get; set; }

        /// <summary>
        /// Whether to start playback muted.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Muted)]
        public bool Muted { get; set; }

        /// <summary>
        /// Gets or sets the current poster image for the player.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Poster)]
        public string Poster { get; set; }

        /// <summary>
        /// Defines the manual structure of the protection data. If defined, it will override the usage of
        /// ProtectionServerUrl and ProtectionHttpRequestHeaders.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ProtectionData)]
        public string ProtectionData { get; set; }

        /// <summary>
        /// Defines the protection token for the http header that is sent to the server.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ProtectionHttpRequestHeaders)]
        public string ProtectionHttpRequestHeaders { get; set; }

        /// <summary>
        /// Defines the server url of the DRM protection.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ProtectionServerUrl)]
        public string ProtectionServerUrl { get; set; }

        /// <summary>
        /// Defines the encoding type used for the DRM protection.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ProtectionType)]
        public VideoProtectionType ProtectionType { get; set; }

        /// <summary>
        /// Force an aspect ratio for all videos. The format is 'w:h' - e.g. '16:9' or '4:3'. If this is not specified then
        /// the default for HTML5 and Vimeo is to use the native resolution of the video. As dimensions are not available
        /// from YouTube via SDK, 16:9 is forced as a sensible default.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Ratio)]
        public string Ratio { get; set; }

        /// <summary>
        /// Reset the playback to the start once playback is complete.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ResetOnEnd)]
        public bool ResetOnEnd { get; set; }

        /// <summary>
        /// The time, in seconds, to seek when a user hits fast forward or rewind.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.SeekTime)]
        public int SeekTime { get; set; }

        /// <summary>
        /// If defined the video will run in streaming mode.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.StreamingLibrary)]
        public StreamingLibrary StreamingLibrary { get; set; }

        /// <summary>
        /// A number, between 0 and 1, representing the initial volume of the player.
        /// </summary>
        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Volume)]
        public double Volume { get; set; }

        #region IContentBlock Members

        public override string Name => "Video Block";

        public override Type EditorType => typeof(VideoBlockEditor);

        public override Type DisplayType => typeof(VideoBlockDisplay);

        #endregion IContentBlock Members
    }
}