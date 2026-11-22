using Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Components;

/// <summary>Shows whether a queued message is waiting or has been sent.</summary>
public partial class QueuedEmailStatusBadge
{
    /// <summary>The queued message. Sent when <see cref="QueuedEmailSummary.SentOnUtc"/> has a value.</summary>
    [Parameter, EditorRequired]
    public QueuedEmailSummary Email { get; set; }
}
