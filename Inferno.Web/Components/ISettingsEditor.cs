using Inferno.Web.Configuration.Entities;

namespace Inferno.Web.Components
{
    public interface ISettingsEditor
    {
        Setting Data { get; set; }

        string Save();
    }
}