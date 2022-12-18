namespace Inferno.Web.Areas.Admin.Configuration.Components
{
    public partial class SiteSettingsEditor
    {
        private string selectedTab = "General";

        private Task OnSelectedTabChanged(string name)
        {
            selectedTab = name;
            return Task.CompletedTask;
        }
    }
}