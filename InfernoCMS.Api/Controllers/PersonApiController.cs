﻿using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using InfernoCMS.Data.Entities;

namespace InfernoCMS.Controllers.Api
{
    public class PersonApiController : GenericODataController<Person, int>
    {
        public PersonApiController(IRepository<Person> repository)
            : base(repository)
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