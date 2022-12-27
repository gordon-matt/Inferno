using Microsoft.OData.ModelBuilder.Core.V1;

namespace Inferno.Web.Navigation
{
    public class NavigationItemBuilder : NavigationBuilder
    {
        private readonly MenuItem item;

        public NavigationItemBuilder()
        {
            item = new MenuItem();
        }

        public NavigationItemBuilder Caption(string caption)
        {
            item.Text = caption;
            return this;
        }

        public NavigationItemBuilder Position(string position)
        {
            item.Position = position;
            return this;
        }

        public NavigationItemBuilder Url(string url)
        {
            item.Url = url;
            return this;
        }

        public NavigationItemBuilder CssClass(string className)
        {
            item.CssClass = className;
            return this;
        }

        public NavigationItemBuilder Icons(params string[] icons)
        {
            item.Icons = item.Icons.Concat(icons);
            return this;
        }

        public override IEnumerable<MenuItem> Build()
        {
            item.Items = base.Build();
            return new[] { item };
        }

        public NavigationItemBuilder Permission(params string[] policies)
        {
            item.Policies = item.Policies.Concat(policies);
            return this;
        }
    }
}