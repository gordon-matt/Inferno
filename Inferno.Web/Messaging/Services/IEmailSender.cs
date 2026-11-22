using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Inferno.Web.Messaging.Configuration;

namespace Inferno.Web.Messaging.Services;

/// <summary>Sends mail through the tenant's <see cref="Configuration.SmtpSettings"/>.</summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends <paramref name="mailMessage"/>. When a host is configured, the client uses that host, port, SSL flag, and credentials.
    /// A message with no From address takes the username when that username is an email address.
    /// </summary>
    /// <param name="mailMessage">The message to send. Its From address may be set as a side effect.</param>
    void Send(MailMessage mailMessage);

    /// <summary>Sends an HTML message, UTF-8 encoded, by calling <see cref="Send(MailMessage)"/>.</summary>
    /// <param name="subject">The subject.</param>
    /// <param name="body">The HTML body.</param>
    /// <param name="toEmailAddress">The recipient address.</param>
    void Send(string subject, string body, string toEmailAddress);
}

/// <inheritdoc cref="IEmailSender" />
public class DefaultEmailSender : IEmailSender
{
    private static readonly Regex ValidEmail = new(@"[\w.+-]+@([\w-]+\.)+[\w-]{2,}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly SmtpSettings smtpSettings;

    /// <summary>Creates the sender.</summary>
    /// <param name="smtpSettings">The SMTP settings. <see langword="null"/> or an empty host leaves the client on its defaults.</param>
    public DefaultEmailSender(SmtpSettings smtpSettings)
    {
        this.smtpSettings = smtpSettings;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <summary>Whether <paramref name="mailAddress"/> matches the sender's email pattern.</summary>
    /// <param name="mailAddress">The address to test.</param>
    /// <returns><see langword="false"/> when <paramref name="mailAddress"/> is null, empty, or does not match.</returns>
    public static bool IsValidEmailAddress(string mailAddress) =>
        !string.IsNullOrEmpty(mailAddress) && ValidEmail.IsMatch(mailAddress);
}
