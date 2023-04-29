using Inferno.Security;
using Inferno.Security.Membership;
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
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UserApiController : ODataController
    {
        private readonly ILogger logger;
        private readonly IWorkContext workContext;
        private readonly IAuthorizationService authorizationService;

        protected IMembershipService Service { get; private set; }

        private readonly Lazy<MembershipSettings> membershipSettings;

        public UserApiController(
            IMembershipService service,
            Lazy<MembershipSettings> membershipSettings,
            ILoggerFactory loggerFactory,
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            this.Service = service;
            this.membershipSettings = membershipSettings;
            this.logger = loggerFactory.CreateLogger<UserApiController>();
            this.workContext = workContext;
            this.authorizationService = authorizationService;
        }

        public virtual async Task<IActionResult> Get(ODataQueryOptions<InfernoUser> options)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersRead))
            {
                return Unauthorized();
            }

            var query = (await Service.GetAllUsersAsync(workContext.CurrentTenant.Id)).AsQueryable();
            var results = options.ApplyTo(query);

            var response = await Task.FromResult((results as IQueryable<InfernoUser>).ToHashSet());
            return Ok(response);
        }

        [EnableQuery]
        public virtual async Task<IActionResult> Get([FromODataUri] string key)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersRead))
            {
                return Unauthorized();
            }

            var entity = await Service.GetUserByIdAsync(key);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        public virtual async Task<IActionResult> Put([FromODataUri] string key, [FromBody] InfernoUser entity)
        {
            if (entity == null)
            {
                return BadRequest();
            }

            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersWrite))
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
                await Service.UpdateUserAsync(entity);
            }
            catch (DbUpdateConcurrencyException x)
            {
                logger.LogError(new EventId(), x, x.Message);

                if (!await CheckEntityExistsAsync(key))
                {
                    return NotFound();
                }
                else { throw; }
            }

            return Updated(entity);
        }

        public virtual async Task<IActionResult> Post([FromBody] InfernoUser entity)
        {
            if (entity == null)
            {
                return BadRequest();
            }

            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersWrite))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string password = Password.Generate(
                membershipSettings.Value.GeneratedPasswordLength,
                membershipSettings.Value.GeneratedPasswordNumberOfNonAlphanumericChars);

            entity.TenantId = workContext.CurrentTenant.Id;
            await Service.InsertUserAsync(entity, password);

            return Created(entity);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public virtual async Task<IActionResult> Patch([FromODataUri] string key, Delta<InfernoUser> patch)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersWrite))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            InfernoUser entity = await Service.GetUserByIdAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            patch.Patch(entity);

            try
            {
                await Service.UpdateUserAsync(entity);
            }
            catch (DbUpdateConcurrencyException x)
            {
                logger.LogError(new EventId(), x, x.Message);

                if (!await CheckEntityExistsAsync(key))
                {
                    return NotFound();
                }
                else { throw; }
            }

            return Updated(entity);
        }

        public virtual async Task<IActionResult> Delete([FromODataUri] string key)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersWrite))
            {
                return Unauthorized();
            }

            InfernoUser entity = await Service.GetUserByIdAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            await Service.DeleteUserAsync(key);

            return NoContent();
        }

        protected virtual async Task<bool> CheckEntityExistsAsync(string key)
        {
            var user = await Service.GetUserByIdAsync(key);
            return user != null;
        }

        public virtual async Task<IActionResult> GetUsersInRole(
            [FromODataUri] string roleId,
            ODataQueryOptions<InfernoUser> options)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersRead))
            {
                return Unauthorized();
            }

            var query = (await Service.GetUsersByRoleIdAsync(roleId)).AsQueryable();
            var results = options.ApplyTo(query);

            var response = await Task.FromResult((results as IQueryable<InfernoUser>).ToHashSet());
            return Ok(response);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AssignUserToRoles([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersWrite))
            {
                return Unauthorized();
            }

            string userId = (string)parameters["userId"];
            var roleIds = (IEnumerable<string>)parameters["roles"];

            await Service.AssignUserToRolesAsync(workContext.CurrentTenant.Id, userId, roleIds);

            return Ok();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ChangePassword([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.MembershipUsersWrite))
            {
                return Unauthorized();
            }

            string userId = (string)parameters["userId"];
            string password = (string)parameters["password"];

            try
            {
                await Service.ChangePasswordAsync(userId, password);
                return Ok();
            }
            catch (Exception x)
            {
                return StatusCode(500, x.Message);
            }
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
}