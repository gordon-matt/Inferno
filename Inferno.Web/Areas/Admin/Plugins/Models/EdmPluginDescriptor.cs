using Inferno.Plugins;

namespace Inferno.Web.Areas.Admin.Plugins.Models
{
    public class EdmPluginDescriptor
    {
        /// <summary>
        /// OData v4 requires a non-null primitive key. Plugin SystemNames frequently
        /// contain dots (e.g. <c>Inferno.Plugins.Foo</c>) which the OData URL parser
        /// chokes on, so we encode/decode them by replacing '.' with '-' here.
        /// </summary>
        public string Id { get; set; }

        public string Group { get; set; }

        public string FriendlyName { get; set; }

        public string SystemName { get; set; }

        public string Version { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool Installed { get; set; }

        public IEnumerable<int> LimitedToTenants { get; set; }

        public static implicit operator EdmPluginDescriptor(PluginDescriptor other) => new()
        {
            Id = other.SystemName.Replace('.', '-'),
            Group = other.Group,
            FriendlyName = other.FriendlyName,
            SystemName = other.SystemName,
            Version = other.Version,
            Author = other.Author,
            Description = other.Description,
            DisplayOrder = other.DisplayOrder,
            Installed = other.Installed,
            LimitedToTenants = other.LimitedToTenants ?? Enumerable.Empty<int>()
        };
    }
}
