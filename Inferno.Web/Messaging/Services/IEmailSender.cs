using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Inferno.Web.Messaging.Configuration;

namespace Inferno.Web.Messaging.Services;

public interface IEmailSender
{
    void Send(MailMessage mailMessage);

    void Send(string subject, string body, string toEmailAddress);
}

public class DefaultEmailSender : IEmailSender
{
    private static readonly Regex ValidEmail = new(@"[\w.+-]+@([\w-]+\.)+[\w-]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly SmtpSettings smtpSettings;

    public DefaultEmailSender(SmtpSettings smtpSettings)
    {
        this.smtpSettings = smtpSettings;
    }

    public void Send(MailMessage mailMessage)
    {
        using var smtpClient = new SmtpClient();
        if (smtpSettings != null && !string.IsNullOrEmpty(smtpSettings.Host))
        {
            smtpClient.UseDefaultCredentials = smtpSettings.UseDefaultCredentials;
            smtpClient.Host = smtpSettings.Host;
            smtpClient.Port = smtpSettings.Port;
            smtpClient.EnableSsl = smtpSettings.EnableSsl;
            smtpClient.Credentials = smtpSettings.UseDefaultCredentials
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

            if (mailMessage.From == null && IsValidEmailAddress(smtpSettings.Username))
            {
                mailMessage.From = new MailAddress(smtpSettings.Username, smtpSettings.DisplayName ?? string.Empty);
            }
        }

        smtpClient.Send(mailMessage);
    }

    public void Send(string subject, string body, string toEmailAddress)
    {
        using var mailMessage = new MailMessage
        {
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            Body = body,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        mailMessage.To.Add(toEmailAddress);
        Send(mailMessage);
    }

    public static bool IsValidEmailAddress(string mailAddress) =>
        !string.IsNullOrEmpty(mailAddress) && ValidEmail.IsMatch(mailAddress);
}
