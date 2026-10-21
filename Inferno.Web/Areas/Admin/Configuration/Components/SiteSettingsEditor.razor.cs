namespace Inferno.Web.Areas.Admin.Configuration.Components;

public partial class SiteSettingsEditor
{
    private string selectedTab = "General";

    private void SelectTab(string name) => selectedTab = name;
}