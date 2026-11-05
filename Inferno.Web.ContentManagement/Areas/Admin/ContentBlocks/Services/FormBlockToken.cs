using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

/// <summary>
/// The server-side settings of a form block that a visitor must never be able to change.
/// </summary>
public sealed record FormBlockSettings(
    string ThankYouMessage,
    string RedirectUrl,
    string EmailAddress,
    string FormUrl,
    string Title);

/// <summary>
/// Seals a form block's settings into a tamper-proof token that is rendered with the form,
/// so a submission can only ever use what an editor configured.
/// </summary>
public static class FormBlockToken
{
    /// <summary>The name of the hidden form field that carries the token.</summary>
    public const string FIELD_NAME = "FormToken";

    private const string PROTECTOR_PURPOSE = "Inferno.FormBlock.Settings.v1";

    /// <summary>Seals the settings of a form block.</summary>
    public static string Protect(IDataProtectionProvider provider, FormBlockSettings settings) =>
        provider.CreateProtector(PROTECTOR_PURPOSE).Protect(JsonSerializer.Serialize(settings));

    /// <summary>Opens a token, returning <c>null</c> when it is missing, altered or was not issued by this site.</summary>
    public static FormBlockSettings Unprotect(IDataProtectionProvider provider, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            string json = provider.CreateProtector(PROTECTOR_PURPOSE).Unprotect(token);
            return JsonSerializer.Deserialize<FormBlockSettings>(json);
        }
        catch (Exception ex) when (ex is System.Security.Cryptography.CryptographicException or JsonException)
        {
            return null;
        }
    }
}
