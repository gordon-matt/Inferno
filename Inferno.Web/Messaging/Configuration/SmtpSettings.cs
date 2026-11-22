using Localize;
using Inferno.Web.Messaging.Components;

namespace Inferno.Web.Messaging.Configuration;

/// <summary>The SMTP server used to send this tenant's mail, and how queued mail is batched.</summary>
public class SmtpSettings : Inferno.Web.Configuration.ISettings
{
    /// <summary>Sets the port to 25, the send tries to 3, and the batch size to 50.</summary>
    public SmtpSettings()
    {
        Port = 25;
        MaxTries = 3;
        MessagesPerBatch = 50;
    }

    /// <inheritdoc />
    public string Name => "Messaging: SMTP Settings";

    /// <inheritdoc />
    public bool IsTenantRestricted => false;

    /// <inheritdoc />
    public Type EditorType => typeof(SmtpSettingsEditor);

    /// <summary>The display name used when the message has no From address and the username is an email address.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.DisplayName)]
    public string DisplayName { get; set; }

    /// <summary>The SMTP host. Empty leaves the SMTP client on its default configuration.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.Host)]
    public string Host { get; set; }

    /// <summary>The SMTP port. Defaults to 25.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.Port)]
    public int Port { get; set; }

    /// <summary>Whether the SMTP client uses SSL.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.EnableSsl)]
    public bool EnableSsl { get; set; }

    /// <summary>Whether the SMTP client uses the process credentials instead of <see cref="Username"/> and <see cref="Password"/>.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.UseDefaultCredentials)]
    public bool UseDefaultCredentials { get; set; }

    /// <summary>The SMTP user name. When it is an email address, it is also the From address of a message that has none.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.Username)]
    public string Username { get; set; }

    /// <summary>The SMTP password. Ignored when <see cref="UseDefaultCredentials"/> is set.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.Password)]
    public string Password { get; set; }

    /// <summary>How many send attempts a queued message gets before it is left unsent. Defaults to 3, and a non-positive value is treated as 3.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.MaxTries)]
    public int MaxTries { get; set; }

    /// <summary>How many queued messages one run sends for this tenant. Defaults to 50, and a non-positive value is treated as 50.</summary>
    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.MessagesPerBatch)]
    public int MessagesPerBatch { get; set; }
}
