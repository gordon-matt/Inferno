﻿using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Web.ContentManagement.Areas.Admin.Menus.Entities;
using Microsoft.AspNetCore.Authorization;

namespace Inferno.Web.ContentManagement.Areas.Admin.Menus.Controllers.Api
{
    //[Authorize(Roles = InfernoConstants.Roles.Administrators)]
    [Authorize]
    public class MenuItemApiController : BaseODataController<MenuItem, Guid>
    {
        public MenuItemApiController(IAuthorizationService authorizationService, IRepository<MenuItem> repository)
            : base(authorizationService, repository)
        {
        }

        protected override Guid GetId(MenuItem entity) => entity.Id;

        protected override void SetNewId(MenuItem entity) => entity.Id = Guid.NewGuid();

        protected override string ReadPermission => CmsConstants.Policies.MenusRead;

        protected override string WritePermission => CmsConstants.Policies.MenusWrite;
    }
}