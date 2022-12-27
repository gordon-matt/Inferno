namespace Inferno.Web.Navigation
{
    public class MenuItem
    {
        public string Text { get; set; }

        //public string Description { get; set; }

        public string Url { get; set; }

        public string Href { get; set; }

        public string Position { get; set; }

        public bool Selected { get; set; }

        public IEnumerable<MenuItem> Items { get; set; }

        public IEnumerable<string> Policies { get; set; }

        public string CssClass { get; set; }

        public IEnumerable<string> Icons { get; set; } = Enumerable.Empty<string>();

        public override string ToString() => Text;
    }
}