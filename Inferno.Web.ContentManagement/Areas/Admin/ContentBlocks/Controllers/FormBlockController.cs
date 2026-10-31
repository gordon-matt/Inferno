using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Models;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Controllers;

[Route("inferno-cms")]
public class FormBlockController : Controller
{
    private readonly IFormBlockSubmissionService submissionService;

    public FormBlockController(IFormBlockSubmissionService submissionService)
    {
        this.submissionService = submissionService;
    }

    [HttpPost("form-content-block/save")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Save(IFormCollection form)
    {
        if (!IsSameOrigin())
        {
            return BadRequest();
        }

        var result = await submissionService.SubmitAsync(form, Request.Headers.Referer.ToString());
        bool isAjax = string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        if (isAjax)
        {
            return Json(result);
        }

        string title = result.Success ? "Success" : "Error";
        string html = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8" />
                <meta http-equiv="refresh" content="4;url={System.Net.WebUtility.HtmlEncode(result.RedirectUrl)}" />
                <title>{title}</title>
                <link rel="stylesheet" href="/lib/bootstrap/dist/css/bootstrap.min.css" />
            </head>
            <body class="container py-5">
                <div class="alert {(result.Success ? "alert-success" : "alert-danger")}" role="alert">
                    <h1 class="h4">{title}</h1>
                    <p>{System.Net.WebUtility.HtmlEncode(result.Message)}</p>
                    <a href="{System.Net.WebUtility.HtmlEncode(result.RedirectUrl)}">Continue</a>
                </div>
            </body>
            </html>
            """;
        return Content(html, "text/html");
    }

    private bool IsSameOrigin()
    {
        string origin = Request.Headers.Origin.ToString();
        if (string.IsNullOrEmpty(origin))
        {
            origin = Request.Headers.Referer.ToString();
        }

        if (string.IsNullOrEmpty(origin) || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return string.Equals(uri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase);
    }
}
