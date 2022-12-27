namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks
{
    public interface IContentBlock
    {
        Guid Id { get; set; }

        string Title { get; set; }

        int Order { get; set; }

        bool Enabled { get; set; }

        string Name { get; }

        Guid ZoneId { get; set; }

        Guid? PageId { get; set; }

        bool Localized { get; set; }

        Type EditorType { get; }

        Type DisplayType { get; }

        string CustomDisplayType { get; set; }
    }
}