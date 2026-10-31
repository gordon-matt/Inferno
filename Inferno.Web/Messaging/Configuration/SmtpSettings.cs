using Inferno.Localization.ComponentModel;
using Inferno.Web.Messaging.Components;

namespace Inferno.Web.Messaging.Configuration;

public class SmtpSettings : Inferno.Web.Configuration.ISettings
{
    public SmtpSettings()
    {
        Port = 25;
        MaxTries = 3;
        MessagesPerBatch = 50;
    }

    public string Name => "Messaging: SMTP Settings";

    public bool IsTenantRestricted => false;

    public Type EditorType => typeof(SmtpSettingsEditor);

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.DisplayName)]
    public string DisplayName { get; set; }

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.Host)]
    public string Host { get; set; }

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.Port)]
    public int Port { get; set; }

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.EnableSsl)]
    public bool EnableSsl { get; set; }

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.UseDefaultCredentials)]
    public bool UseDefaultCredentials { get; set; }

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.Username)]
    public string Username { get; set; }

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.Password)]
    public string Password { get; set; }

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.MaxTries)]
    public int MaxTries { get; set; }

    [LocalizedDisplayName(Inferno.Web.InfernoWebLocalizableStrings.Settings.Smtp.MessagesPerBatch)]
    public int MessagesPerBatch { get; set; }
}
