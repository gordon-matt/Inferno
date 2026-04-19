using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Threading;
using Inferno.Web.Security;
using Inferno.Web.Security.Membership;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Areas.Admin.Membership.Controllers.Api
{
    [Authorize]
    public class RoleApiController : ODataController
    {
        private readonly ILogger logger;
        private readonly IWorkContext workContext;
        private readonly IAuthorizationService authorizationService;

        protected IMembershipService Service { get; private set; }

        public RoleApiController(
            IMembershipService service,
            ILoggerFactory loggerFactory,
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            this.Service = service;
            this.logger = loggerFactory.CreateLogger<RoleApiController>();
            this.workContext = workContext;
            this.authorizationService = authorizationService;
        }

        public virtual async Task<IActionResult> Get(ODataQueryOptions<InfernoRole> options)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipRolesRead))
            {
                return Unauthorized();
            }

            //var settings = new ODataValidationSettings()
            //{
            //    AllowedQueryOptions = AllowedQueryOptions.All
            //};
            //options.Validate(settings);

            var results = options.ApplyTo((await Service.GetAllRolesAsync(workContext.CurrentTenant.Id)).AsQueryable());
            var response = await Task.FromResult((results as IQueryable<InfernoRole>).ToHashSet());
            return Ok(response);
        }

        [EnableQuery]
        public virtual async Task<IActionResult> Get([FromODataUri] string key)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipRolesRead))
            {
                return Unauthorized();
            }

            var entity = await Service.GetRoleByIdAsync(key);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        public virtual async Task<IActionResult> Put([FromODataUri] string key, [FromBody] InfernoRole entity)
        {
            if (entity == null)
            {
                return BadRequest();
            }

            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipRolesWrite))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!key.Equals(entity.Id))
            {
                return BadRequest();
            }

            try
            {
                await Service.UpdateRoleAsync(entity);
            }
            catch (DbUpdateConcurrencyException x)
            {
                logger.LogError(new EventId(), x, x.Message);

                if (!EntityExists(key))
                {
                    return NotFound();
                }
                else { throw; }
            }

            return Updated(entity);
        }

        public virtual async Task<IActionResult> Post([FromBody] InfernoRole entity)
        {
            if (entity == null)
            {
                return BadRequest();
            }

            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipRolesWrite))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            entity.TenantId = workContext.CurrentTenant.Id;
            await Service.InsertRoleAsync(entity);

            return Created(entity);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public virtual async Task<IActionResult> Patch([FromODataUri] string key, Delta<InfernoRole> patch)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipRolesWrite))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            InfernoRole entity = await Service.GetRoleByIdAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            patch.Patch(entity);

            try
            {
                await Service.UpdateRoleAsync(entity);
            }
            catch (DbUpdateConcurrencyException x)
            {
                logger.LogError(new EventId(), x, x.Message);

                if (!EntityExists(key))
                {
                    return NotFound();
                }
                else { throw; }
            }

            return Updated(entity);
        }

        public virtual async Task<IActionResult> Delete([FromODataUri] string key)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipRolesWrite))
            {
                return Unauthorized();
            }

            InfernoRole entity = await Service.GetRoleByIdAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            await Service.DeleteRoleAsync(key);

            return NoContent();
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetRolesForUser([FromODataUri] string userId)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipRolesRead))
            {
                return Unauthorized();
            }

            var results = (await Service.GetRolesForUserAsync(userId)).Select(x => new EdmRole
            {
                Id = x.Id,
                Name = x.Name
            });

            var response = await Task.FromResult(results);
            return Ok(response);
        }

        //[HttpPost]
        //public virtual async Task<IActionResult> AssignPermissionsToRole([FromBody] ODataActionParameters parameters)
        //{
        //    if (!await AuthorizeAsync(InfernoWebPolicies.MembershipRolesWrite))
        //    {
        //        return Unauthorized();
        //    }

        //    string roleId = (string)parameters["roleId"];
        //    var permissionIds = (IEnumerable<string>)parameters["permissions"];

        //    await Service.AssignPermissionsToRole(roleId, permissionIds);

        //    return Ok();
        //}

        protected virtual bool EntityExists(string key)
        {
            return AsyncHelper.RunSync(() => Service.GetUserByIdAsync(key)) != null;
        }

        protected virtual async Task<bool> AuthorizeAsync(string policyName)
        {
            if (authorizationService == null || string.IsNullOrEmpty(policyName))
            {
                return true;
            }

            return (await authorizationService.AuthorizeAsync(User, policyName)).Succeeded;
        }
    }

    public struct EdmRole
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
