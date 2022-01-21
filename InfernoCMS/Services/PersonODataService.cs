using Inferno.Web.OData;
using InfernoCMS.Data.Entities;

namespace InfernoCMS.Services
{
    public class PersonODataService : RadzenODataService<Person>
    {
        public PersonODataService(IHttpContextAccessor httpContextAccessor)
            : base("PersonApi", httpContextAccessor)
        {
        }
    }
}