namespace Inferno.Web.Configuration;

public class InfernoWebOptions
{
    public bool IgnoreStartupTasks { get; set; }

    /// <summary>
    /// The host's public layout. Library pages such as the page preview cannot name a layout in the host project,
    /// so they use <see cref="Components.PublicSiteLayout"/>, which renders this one. Null renders the page bare.
    /// </summary>
    public Type PublicLayout { get; set; }
}
