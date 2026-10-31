using Dependo;
using Inferno.Tasks;
using Inferno.Tenants.Services;
using Inferno.Web.Configuration.Services;
using Inferno.Web.Messaging.Configuration;
using Inferno.Web.Messaging.Services;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Messaging;

public class ProcessQueuedMailTask : ITask
{
    public string Name => "Process Queued Mail Task";

    public int DefaultInterval => 600;

    public void Execute()
    {
        var providers = DependoResolver.Instance.ResolveAll<IQueuedMessageProvider>();
        var tenantService = DependoResolver.Instance.Resolve<ITenantService>();
        var settingService = DependoResolver.Instance.Resolve<ISettingService>();
        var logger = DependoResolver.Instance.Resolve<ILoggerFactory>().CreateLogger(GetType());

        var tenantIds = tenantService.Find(new Extenso.Data.Entity.SearchOptions<Inferno.Tenants.Entities.Tenant>
        {
            Query = x => true
        }).Select(x => x.Id).ToList();

        foreach (int tenantId in tenantIds)
        {
            var smtpSettings = settingService.GetSettings<SmtpSettings>(tenantId);
            int maxTries = smtpSettings != null && smtpSettings.MaxTries > 0 ? smtpSettings.MaxTries : 3;
            int messagesPerBatch = smtpSettings != null && smtpSettings.MessagesPerBatch > 0 ? smtpSettings.MessagesPerBatch : 50;
            var emailSender = new DefaultEmailSender(smtpSettings);

            foreach (var provider in providers)
            {
                var queuedEmails = provider.GetQueuedEmails(tenantId, maxTries, messagesPerBatch);
                foreach (var queuedEmail in queuedEmails)
                {
                    try
                    {
                        using var mailMessage = queuedEmail.GetMailMessage();
                        emailSender.Send(mailMessage);
                        provider.OnSendSuccess(queuedEmail);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error sending queued e-mail for tenant {TenantId}", tenantId);
                        provider.OnSendError(queuedEmail);
                    }
                }
            }
        }
    }
}
