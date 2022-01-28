namespace Inferno.Web.Navigation
{
    public interface INavigationManager
    {
        IEnumerable<MenuItem> BuildMenu(string menuName);

        string GetUrl(string menuItemUrl);
    }
}