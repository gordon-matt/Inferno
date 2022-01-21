using Dependo;

namespace Inferno.Web.Mvc.Themes
{
    public class CurrentThemeStateProvider : IWorkContextStateProvider
    {
        public Func<IWorkContext, T> Get<T>(string name)
        {
            if (name == InfernoWebConstants.StateProviders.CurrentTheme)
            {
                string currentTheme = EngineContext.Current.Resolve<IThemeContext>().WorkingTheme;
                return ctx => (T)(object)currentTheme;
            }
            return null;
        }
    }
}