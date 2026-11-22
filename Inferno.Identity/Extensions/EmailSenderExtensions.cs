using System.Text.Encodings.Web;

namespace Inferno.Identity.Services;

/// <summary>Builds account-confirmation mail for <see cref="IEmailSender"/>.</summary>
public static class EmailSenderExtensions
{
    /// <summary>Sends an account-confirmation message that links to <paramref name="link"/>.</summary>
    /// <param name="emailSender">The sender that delivers the message.</param>
    /// <param name="email">The recipient address.</param>
    /// <param name="link">The confirmation URL. It is HTML-encoded in the message body.</param>
    /// <returns>A task that completes when <see cref="IEmailSender.SendEmailAsync"/> completes.</returns>
    public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link) => emailSender.SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
}