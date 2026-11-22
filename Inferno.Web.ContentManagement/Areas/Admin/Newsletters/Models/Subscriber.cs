using Extenso.Data.Entity;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Models;

/// <summary>
/// Represents a newsletter subscriber. Subscribers are tracked via a profile entry on
/// the underlying <see cref="Inferno.Security.Membership.InfernoUser"/> rather than
/// having their own database table.
/// </summary>
public class Subscriber : BaseEntity<string>
{
    /// <summary>The name shown for the subscriber.</summary>
    public string Name { get; set; }

    /// <summary>The subscriber's email address.</summary>
    public string Email { get; set; }
}