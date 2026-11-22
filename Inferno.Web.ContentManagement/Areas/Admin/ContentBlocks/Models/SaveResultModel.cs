namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Models;

/// <summary>The outcome of a form-block submission, shown to the visitor.</summary>
public class SaveResultModel
{
    /// <summary><see langword="true"/> when the form was accepted.</summary>
    public bool Success { get; set; }

    /// <summary>The message to show.</summary>
    public string Message { get; set; }

    /// <summary>Where the visitor is sent next.</summary>
    public string RedirectUrl { get; set; }
}
