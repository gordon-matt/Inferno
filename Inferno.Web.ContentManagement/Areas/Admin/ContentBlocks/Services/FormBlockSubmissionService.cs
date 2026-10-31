using System.Net.Mail;
using System.Text;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Models;
using Inferno.Web.Messaging.Services;
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
        "__RequestVerificationToken"
    };

    private readonly IEmailSender emailSender;
    private readonly IEnumerable<IFormBlockProcessor> processors;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<FormBlockSubmissionService> logger;

    public FormBlockSubmissionService(
        IEmailSender emailSender,
        IEnumerable<IFormBlockProcessor> processors,
        IHttpClientFactory httpClientFactory,
        ILogger<FormBlockSubmissionService> logger)
    {
        this.emailSender = emailSender;
        this.processors = processors;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public async Task<SaveResultModel> SubmitAsync(IFormCollection form, string referrer)
    {
        string thankYouMessage = form["ThankYouMessage"];
        string redirectUrl = form["RedirectUrl"];
        string emailAddress = form["EmailAddress"];
        string contentBlockTitle = form["ContentBlockTitle"];
        string formUrl = form["FormUrl"];
        string destination = string.IsNullOrWhiteSpace(redirectUrl) ? "/" : redirectUrl;

        if (!string.IsNullOrWhiteSpace(formUrl))
        {
            try
            {
                using var client = httpClientFactory.CreateClient();
                using var content = new FormUrlEncodedContent(form.Keys.ToDictionary(k => k, k => form[k].ToString()));
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
