using Dependo;
using Inferno.Web.Navigation;
using Microsoft.Extensions.Localization;

namespace Inferno.Web.Areas.Admin
{
    public class AdminNavigationProvider : INavigationProvider
    {
        public AdminNavigationProvider()
        {
            T = EngineContext.Current.Resolve<IStringLocalizer>();
        }

        public IStringLocalizer T { get; set; }

        #region INavigationProvider Members

        public string MenuName => InfernoWebConstants.Areas.Admin;

        public void GetNavigation(NavigationBuilder builder)
        {
            builder.Add(T[InfernoWebLocalizableStrings.General.Home], "0", BuildDashboardMenu);
        }

        private static void BuildDashboardMenu(NavigationItemBuilder builder)
        {
            builder.Icon("fa fa-home");
        }

        #endregion INavigationProvider Members
    }
}