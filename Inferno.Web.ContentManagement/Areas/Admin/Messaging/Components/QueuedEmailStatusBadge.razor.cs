using Inferno.Web.ContentManagement.Areas.Admin.Messaging.Services;
using Microsoft.AspNetCore.Components;

namespace Inferno.Web.ContentManagement.Areas.Admin.Messaging.Components;

public partial class QueuedEmailStatusBadge
{
    [Parameter, EditorRequired]
    public QueuedEmailSummary Email { get; set; }
}
