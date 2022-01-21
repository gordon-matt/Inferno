using Extenso.Data.Entity;

namespace Inferno.Security.Membership
{
    public class InfernoUserProfileEntry : BaseEntity<string>
    {
        public string UserId { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}