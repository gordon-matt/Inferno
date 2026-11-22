namespace Inferno.Web.Configuration;

/// <summary>Host options that configuration can bind and that startup code reads.</summary>
public class InfernoWebOptions
{
    /// <summary>When <see langword="true"/>, <see cref="Inferno.Web.Infrastructure.ApplicationBuilderExtensions.UseInferno"/> does not run startup tasks.</summary>
    public bool IgnoreStartupTasks { get; set; }

    /// <summary>
    /// The host's public layout. Library pages such as the page preview cannot name a layout in the host project,
    /// so they use <see cref="Inferno.Web.Components.PublicSiteLayout"/>, which renders this one. <see langword="null"/> renders the page bare.
    /// </summary>
    public Type PublicLayout { get; set; }
}
