using Extenso.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Navigation
{
    public class NavigationManager : INavigationManager
    {
        private readonly IEnumerable<INavigationProvider> providers;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly ILogger logger;
        private readonly IWebHelper webHelper;
        private readonly IWorkContext workContext;

        public NavigationManager(
            IEnumerable<INavigationProvider> providers,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.logger = loggerFactory.CreateLogger<NavigationManager>();
            this.providers = providers;
            this.webHelper = webHelper;
            this.workContext = workContext;
        }

        #region INavigationManager Members

        public IEnumerable<MenuItem> BuildMenu(string menuName)
        {
            var httpContext = httpContextAccessor.HttpContext;
            var sources = GetSources(menuName);
            string url = UriHelper.BuildAbsolute(httpContext.Request.Scheme, httpContext.Request.Host);
            return FinishMenu(Reduce(Merge(sources), workContext).ToArray(), url);
        }

        private IEnumerable<IEnumerable<MenuItem>> GetSources(string menuName)
        {
            foreach (var provider in providers.Where(x => x.MenuName == menuName))
            {
                var builder = new NavigationBuilder();
                IEnumerable<MenuItem> items = null;
                try
                {
                    provider.GetNavigation(builder);
                    items = builder.Build();
                }
                catch (Exception x)
                {
                    logger.LogError(
                        new EventId(), x, "Unexpected error while querying a navigation provider. It was ignored. The menu provided by the provider may not be complete.");
                }

                if (items != null)
                {
                    yield return items;
                }
            }
        }

        private IEnumerable<MenuItem> FinishMenu(IEnumerable<MenuItem> menuItems, string currentUrl)
        {
            foreach (var menuItem in menuItems)
            {
                menuItem.Href = GetUrl(menuItem.Url);

                if (currentUrl.Equals(menuItem.Href))
                {
                    menuItem.Selected = true;
                }

                menuItem.Items = FinishMenu(menuItem.Items.ToArray(), currentUrl);
            }

            return menuItems;
        }

        private IEnumerable<MenuItem> Reduce(IEnumerable<MenuItem> items, IWorkContext workContext)
        {
            foreach (var item in items)
            {
                yield return new MenuItem
                {
                    Items = Reduce(item.Items, workContext),
                    Position = item.Position,
                    Text = item.Text,
                    CssClass = item.CssClass,
                    Icons = item.Icons,
                    Url = item.Url,
                    Href = item.Href
                };
            }
        }

        private static IEnumerable<MenuItem> Merge(IEnumerable<IEnumerable<MenuItem>> sources)
        {
            var comparer = new MenuItemComparer();
            var orderer = new FlatPositionComparer();

            return sources.SelectMany(x => x).ToArray()

                // group same menus
                .GroupBy(key => key, (key, items) => Join(items.ToList()), comparer)

                // group same position
                .GroupBy(item => item.Position)

                // order position groups by position
                .OrderBy(positionGroup => positionGroup.Key, orderer)

                // ordered by item text in the postion group
                .SelectMany(positionGroup => positionGroup.OrderBy(item => item.Text == null ? "" : item.Text));
        }

        private static MenuItem Join(IEnumerable<MenuItem> items)
        {
            if (items.Count() < 2)
                return items.Single();

            var joined = new MenuItem
            {
                Text = items.First().Text,
                CssClass = items.Select(x => x.CssClass).FirstOrDefault(x => !string.IsNullOrEmpty(x)),
                Icons = items.Select(x => x.Icons).FirstOrDefault(x => !x.IsNullOrEmpty()),
                Url = items.Select(x => x.Url).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                Href = items.Select(x => x.Href).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                Items = Merge(items.Select(x => x.Items)).ToArray(),
                Position = SelectBestPositionValue(items.Select(x => x.Position))
            };
            return joined;
        }

        private static string SelectBestPositionValue(IEnumerable<string> positions)
        {
            var comparer = new FlatPositionComparer();
            return positions.Aggregate(string.Empty, (agg, pos) =>
                string.IsNullOrEmpty(agg)
                    ? pos
                    : string.IsNullOrEmpty(pos)
                            ? agg
                            : comparer.Compare(agg, pos) < 0 ? agg : pos);
        }

        public string GetUrl(string menuItemUrl)
        {
            if (string.IsNullOrEmpty(menuItemUrl))
            {
                return null;
            }

            string url = menuItemUrl;

            if (!string.IsNullOrEmpty(url) &&
                !(url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("javascript:") || url.StartsWith("#") || url.StartsWith("/")))
            {
                if (url.StartsWith("~/"))
                {
                    url = url.Substring(2);
                }
                var appPath = webHelper.WebRootPath;
                if (appPath == "/")
                {
                    appPath = string.Empty;
                }
                url = string.Format("{0}/{1}", appPath, url);
            }
            return url;
        }

        #endregion INavigationManager Members
    }
}