namespace Inferno.Identity.Services;

/// <summary>Sends email for account confirmation and password reset.</summary>
public interface IEmailSender
{
    /// <summary>Sends <paramref name="message"/> to <paramref name="email"/>.</summary>
    /// <param name="email">The recipient address.</param>
    /// <param name="subject">The message subject.</param>
    /// <param name="message">The message body.</param>
    /// <returns>A task that completes when the message has been sent.</returns>
    Task SendEmailAsync(string email, string subject, string message);
}