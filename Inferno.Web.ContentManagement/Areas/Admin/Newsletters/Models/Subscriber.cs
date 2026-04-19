using Extenso.Data.Entity;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Models
{
    /// <summary>
    /// Represents a newsletter subscriber. Subscribers are tracked via a profile entry on
    /// the underlying <see cref="Inferno.Security.Membership.InfernoUser"/> rather than
    /// having their own database table.
    /// </summary>
    public class Subscriber : BaseEntity<string>
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}
