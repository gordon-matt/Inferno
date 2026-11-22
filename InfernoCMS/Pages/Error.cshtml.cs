using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace InfernoCMS.Pages;

/// <summary>Shows the request id for an unhandled error.</summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    /// <summary>The current request id, from the activity or the HTTP trace identifier.</summary>
    public string RequestId { get; set; }

    /// <summary>Whether <see cref="RequestId"/> has text to show.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    private readonly ILogger<ErrorModel> _logger;

    /// <summary>Creates the page with <paramref name="logger"/>.</summary>
    /// <param name="logger">The page logger. This handler does not write to it.</param>
    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    /// <summary>Stores the current activity id, or the HTTP trace identifier when there is no activity.</summary>
    public void OnGet() => RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
}