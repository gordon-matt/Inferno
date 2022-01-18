using Inferno.Data.Entities;

namespace Inferno.Services
{
    public class PersonODataService : GenericODataService<Person>
    {
        public PersonODataService() : base("PersonApi")
        {
        }
    }
}