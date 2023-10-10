using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using InfernoCMS.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace InfernoCMS.Controllers.Api
{
    [Authorize]
    public class PersonApiController : GenericODataController<Person, int>
    {
        public PersonApiController(IAuthorizationService authorizationService, IRepository<Person> repository)
            : base(authorizationService, repository)
        {
        }

        protected override int GetId(Person entity)
        {
            return entity.Id;
        }

        protected override void SetNewId(Person entity)
        {
        }
    }
}