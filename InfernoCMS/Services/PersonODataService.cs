using InfernoCMS.Data.Entities;

namespace InfernoCMS.Services
{
    public class PersonODataService : GenericODataService<Person>
    {
        public PersonODataService() : base("PersonApi")
        {
        }
    }
}