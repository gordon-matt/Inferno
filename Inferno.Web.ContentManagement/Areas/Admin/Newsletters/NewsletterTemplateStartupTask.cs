using Dependo;
using Extenso.Data.Entity;
using Inferno.Tenants.Entities;
using Inferno.Tenants.Services;
using Inferno.Web.Infrastructure;
using Inferno.Web.ContentManagement.Areas.Admin.Messaging;
using Inferno.Web.Messaging.Entities;
using Inferno.Web.Messaging.Services;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters;

/// <summary>
/// Creates the subscribe and unsubscribe templates for each tenant the first time the site starts.
/// </summary>
public class NewsletterTemplateStartupTask : IStartupTask
{
    public int Order => 200;

    public async Task ExecuteAsync()
    {
        var tenantService = DependoResolver.Instance.Resolve<ITenantService>();
        var templateService = DependoResolver.Instance.Resolve<IMessageTemplateService>();
        var versionService = DependoResolver.Instance.Resolve<IMessageTemplateVersionService>();

        IEnumerable<Tenant> tenants;
        using (var connection = tenantService.OpenConnection())
        {
            tenants = connection.Query().ToList();
        }

        foreach (var tenant in tenants)
        {
            await EnsureAsync(
                templateService,
                versionService,
                tenant.Id,
                NewsletterEmailConsumer.SubscribedTemplate,
                "Thank you for subscribing",
                MessageTemplateDocuments.Email("<p>Thank you, [UserName]. You are signed up at [Email].</p>"));

            await EnsureAsync(
                templateService,
                versionService,
                tenant.Id,
                NewsletterEmailConsumer.UnsubscribedTemplate,
                "You have unsubscribed",
                MessageTemplateDocuments.Email("<p>[UserName], you have been removed from the mailing list.</p>"));
        }
    }

    private static async Task EnsureAsync(
        IMessageTemplateService templateService,
        IMessageTemplateVersionService versionService,
        int tenantId,
        string name,
        string subject,
        string body)
    {
        var existing = templateService.Find(tenantId, name);
        if (existing != null)
        {
            return;
        }

        var template = await templateService.InsertAsync(new MessageTemplate
        {
            TenantId = tenantId,
            Name = name,
            Enabled = true
        });

        await versionService.InsertAsync(new MessageTemplateVersion
        {
            MessageTemplateId = template.Id,
            CultureCode = null,
            Subject = subject,
            Data = body,
            DateCreatedUtc = DateTime.UtcNow,
            DateModifiedUtc = DateTime.UtcNow
        });
    }
}
