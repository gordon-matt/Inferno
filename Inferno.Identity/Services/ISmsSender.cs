namespace Inferno.Identity.Services;

/// <summary>Sends a text message.</summary>
public interface ISmsSender
{
    /// <summary>Sends <paramref name="message"/> to <paramref name="number"/>.</summary>
    /// <param name="number">The recipient phone number.</param>
    /// <param name="message">The message text.</param>
    /// <returns>A task that completes when the message has been sent.</returns>
    Task SendSmsAsync(string number, string message);
}