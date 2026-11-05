using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Models;
using Inferno.Web.Messaging.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;

public interface IFormBlockSubmissionService
{
    Task<SaveResultModel> SubmitAsync(IFormCollection form, string referrer);
}

public class FormBlockSubmissionService : IFormBlockSubmissionService
{
    private static readonly HashSet<string> ReservedKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id",
        "EnableCaptcha",
        "ThankYouMessage",
        "RedirectUrl",
        "EmailAddress",
        "ContentBlockTitle",
        "FormUrl",
        "X-Requested-With",
        FormBlockToken.FIELD_NAME,
        "__RequestVerificationToken"
    };

    private readonly IEmailSender emailSender;
    private readonly IEnumerable<IFormBlockProcessor> processors;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<FormBlockSubmissionService> logger;
    private readonly IDataProtectionProvider dataProtection;

    public FormBlockSubmissionService(
        IEmailSender emailSender,
        IEnumerable<IFormBlockProcessor> processors,
        IHttpClientFactory httpClientFactory,
        ILogger<FormBlockSubmissionService> logger,
        IDataProtectionProvider dataProtection)
    {
        this.emailSender = emailSender;
        this.processors = processors;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.dataProtection = dataProtection;
    }

    public async Task<SaveResultModel> SubmitAsync(IFormCollection form, string referrer)
    {
        // Only the sealed settings are trusted. Anything a visitor posts for the destination
        // (recipient, target URL, redirect) is ignored, so the form cannot be turned into a mail
        // relay or a way to make the server call internal addresses.
        var settings = FormBlockToken.Unprotect(dataProtection, form[FormBlockToken.FIELD_NAME]);
        if (settings is null)
        {
            return new SaveResultModel
            {
                Success = false,
                Message = "This form is no longer valid. Please reload the page and try again.",
                RedirectUrl = "/"
            };
        }

        string thankYouMessage = settings.ThankYouMessage;
        string emailAddress = settings.EmailAddress;
        string contentBlockTitle = settings.Title;
        string formUrl = settings.FormUrl;
        string destination = string.IsNullOrWhiteSpace(settings.RedirectUrl) ? "/" : settings.RedirectUrl;

        if (!string.IsNullOrWhiteSpace(formUrl))
        {
            if (!await IsPublicHttpUrlAsync(formUrl))
            {
                logger.LogWarning("Form block post to {FormUrl} was refused because it is not a public http(s) address", formUrl);
                return new SaveResultModel
                {
                    Success = false,
                    Message = "This form is not configured correctly.",
                    RedirectUrl = referrer ?? "/"
                };
            }

            try
            {
                using var client = httpClientFactory.CreateClient();
                using var content = new FormUrlEncodedContent(form.Keys.Where(k => !ReservedKeys.Contains(k)).ToDictionary(k => k, k => form[k].ToString()));
                using var response = await client.PostAsync(formUrl, content);
                return new SaveResultModel
                {
                    Success = response.IsSuccessStatusCode,
                    Message = response.IsSuccessStatusCode
                        ? thankYouMessage
                        : $"{(int)response.StatusCode}: {response.ReasonPhrase}",
                    RedirectUrl = destination
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Form block post to {FormUrl} failed", formUrl);
                return new SaveResultModel
                {
                    Success = false,
                    Message = ex.GetBaseException().Message,
                    RedirectUrl = referrer ?? "/"
                };
            }
        }

        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            return new SaveResultModel
            {
                Success = false,
                Message = "This form is not configured with an email address.",
                RedirectUrl = destination
            };
        }

        string body = BuildBody(form);
        using var mailMessage = new MailMessage
        {
            Subject = string.IsNullOrWhiteSpace(contentBlockTitle) ? "Form submission" : contentBlockTitle,
            SubjectEncoding = Encoding.UTF8,
            Body = body,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };

        if (form.Files.Count > 0)
        {
            foreach (var file in form.Files)
            {
                if (file.Length > 0)
                {
                    mailMessage.Attachments.Add(new Attachment(file.OpenReadStream(), file.FileName));
                }
            }
        }

        try
        {
            foreach (var processor in processors)
            {
                processor.Process(new FormCollection(form.ToDictionary(x => x.Key, x => x.Value), form.Files), mailMessage);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A form block processor failed");
        }

        try
        {
            mailMessage.To.Clear();
            mailMessage.To.Add(emailAddress);
            emailSender.Send(mailMessage);
            return new SaveResultModel
            {
                Success = true,
                Message = thankYouMessage,
                RedirectUrl = destination
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Form block email failed");
            return new SaveResultModel
            {
                Success = false,
                Message = ex.GetBaseException().Message,
                RedirectUrl = referrer ?? "/"
            };
        }
    }

    /// <summary>
    /// Accepts only http(s) URLs whose host resolves exclusively to public addresses.
    /// </summary>
    private static async Task<bool> IsPublicHttpUrlAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return false;
        }

        try
        {
            var addresses = await Dns.GetHostAddressesAsync(uri.DnsSafeHost);
            return addresses.Length > 0 && addresses.All(IsPublicAddress);
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
        {
            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetworkV6)
        {
            return !(address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6Multicast
                || (address.GetAddressBytes()[0] & 0xFE) == 0xFC); // fc00::/7 unique local
        }

        var b = address.GetAddressBytes();
        return !(b[0] == 10
            || (b[0] == 172 && b[1] is >= 16 and <= 31)
            || (b[0] == 192 && b[1] == 168)
            || (b[0] == 169 && b[1] == 254)    // link-local, incl. cloud metadata
            || (b[0] == 100 && b[1] is >= 64 and <= 127) // carrier-grade NAT
            || b[0] == 0
            || b[0] >= 224);
    }

    private static string BuildBody(IFormCollection form)
    {
        var builder = new StringBuilder();
        builder.Append("<table>");
        foreach (string key in form.Keys)
        {
            if (ReservedKeys.Contains(key))
            {
                continue;
            }

            builder.Append("<tr><th>")
                .Append(System.Net.WebUtility.HtmlEncode(key))
                .Append("</th><td>")
                .Append(System.Net.WebUtility.HtmlEncode(form[key]))
                .Append("</td></tr>");
        }

        builder.Append("</table>");
        return builder.ToString();
    }
}
