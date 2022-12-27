using Inferno.Localization.ComponentModel;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks;
using Inferno.Web.ContentManagement.Areas.Admin.Media.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Media.ContentBlocks
{
    public class VideoBlock : ContentBlockBase
    {
        public enum VideoType : byte
        {
            Normal = 0,
            Flash = 1,
            Silverlight = 2
        }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ControlId)]
        public string ControlId { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Type)]
        public VideoType Type { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Source)]
        public string Source { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.ShowControls)]
        public bool ShowControls { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.AutoPlay)]
        public bool AutoPlay { get; set; }

        [LocalizedDisplayName(InfernoCmsLocalizableStrings.ContentBlocks.VideoBlock.Loop)]
        public bool Loop { get; set; }

        #region IContentBlock Members

        public override string Name => "Video Block";

        public override Type EditorType => typeof(VideoBlockEditor);

        public override Type DisplayType => typeof(VideoBlockDisplay);

        #endregion IContentBlock Members
    }
}