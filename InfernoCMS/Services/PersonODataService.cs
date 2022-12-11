using Inferno.Web.OData;

namespace InfernoCMS.Services
{
    public class PersonODataService : RadzenODataService<Person>
    {
        public PersonODataService() : base("PersonApi")
        {
        }
    }
}