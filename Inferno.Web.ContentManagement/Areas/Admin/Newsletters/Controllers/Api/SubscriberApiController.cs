using Inferno.Security;
using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;

namespace Inferno.Web.ContentManagement.Areas.Admin.Newsletters.Controllers.Api
{
    [Authorize]
    public class SubscriberApiController : ODataController
    {
        private readonly IMembershipService membershipService;
        private readonly IWorkContext workContext;
        private readonly IAuthorizationService authorizationService;

        public SubscriberApiController(
            IMembershipService membershipService,
            IWorkContext workContext,
            IAuthorizationService authorizationService)
        {
            this.membershipService = membershipService;
            this.workContext = workContext;
            this.authorizationService = authorizationService;
        }

        public virtual async Task<IActionResult> Get(ODataQueryOptions<Subscriber> options)
        {
            if (!await AuthorizeAsync(CmsConstants.Policies.NewsletterRead))
            {
                return Unauthorized();
            }

            int? tenantId = workContext.CurrentTenant?.Id;

            // Look up the user IDs that have opted in to newsletters via their profile entry,
            // then materialize them into lightweight Subscriber projections for the grid.
            var profileEntries = await membershipService.GetProfileEntriesByKeyAndValueAsync(
                tenantId,
                NewsletterUserProfileProvider.Fields.SubscribeToNewsletters,
                bool.TrueString);

            var userIds = profileEntries.Select(x => x.UserId).ToHashSet();
            if (userIds.Count == 0)
            {
                return Ok(Array.Empty<Subscriber>().AsQueryable());
            }

            var users = (await membershipService.GetAllUsersAsync(tenantId))
                .Where(x => userIds.Contains(x.Id))
                .ToList();

            var subscribers = new List<Subscriber>(users.Count);
            foreach (var user in users)
            {
                subscribers.Add(new Subscriber
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = await membershipService.GetUserDisplayNameAsync(user)
                });
            }

            var query = subscribers
                .OrderBy(x => x.Name)
                .AsQueryable();

            return Ok(options.ApplyTo(query));
        }

        public virtual async Task<IActionResult> Get([FromODataUri] string key)
        {
            if (!await AuthorizeAsync(CmsConstants.Policies.NewsletterRead))
            {
                return Unauthorized();
            }

            var entity = await membershipService.GetUserByIdAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            var subscriber = new Subscriber
            {
                Id = entity.Id,
                Email = entity.Email,
                Name = await membershipService.GetUserDisplayNameAsync(entity)
            };

            return Ok(SingleResult.Create(new[] { subscriber }.AsQueryable()));
        }

        public virtual async Task<IActionResult> Delete([FromODataUri] string key)
        {
            if (!await AuthorizeAsync(CmsConstants.Policies.NewsletterWrite))
            {
                return Unauthorized();
            }

            var entity = await membershipService.GetUserByIdAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            // "Deleting" a subscriber simply opts the underlying user out of newsletters; the
            // user account itself is left alone.
            await membershipService.SaveProfileEntryAsync(
                key,
                NewsletterUserProfileProvider.Fields.SubscribeToNewsletters,
                bool.FalseString);

            return NoContent();
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
