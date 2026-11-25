using Inferno.Web.Infrastructure;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters;

/// <summary>
/// Kept so Dependo still finds a startup task type. Newsletter templates are registered as Kourier
/// definitions in the host and created on first send.
/// </summary>
public class NewsletterTemplateStartupTask : IStartupTask
{
    /// <inheritdoc />
    public int Order => 200;

    /// <inheritdoc />
    public Task ExecuteAsync() => Task.CompletedTask;
}
