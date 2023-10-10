using Extenso.Collections;
using Extenso.Data.Entity;
using Inferno.Security.Membership;
using Inferno.Web.ContentManagement.Areas.Admin.Blog.Entities;
using Inferno.Web.ContentManagement.Areas.Admin.Media;
using Inferno.Web.OData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;

namespace Inferno.Web.ContentManagement.Areas.Admin.Blog.Controllers.Api
{
    //[Authorize(Roles = InfernoConstants.Roles.Administrators)]
    public class BlogPostApiController : GenericTenantODataController<BlogPost, Guid>
    {
        private readonly Lazy<IMembershipService> membershipService;
        private readonly Lazy<IRepository<BlogPostTag>> postTagRepository;
        private readonly Lazy<IWorkContext> workContext;

        public BlogPostApiController(
            IAuthorizationService authorizationService,
            IRepository<BlogPost> repository,
            Lazy<IMembershipService> membershipService,
            Lazy<IRepository<BlogPostTag>> postTagRepository,
            Lazy<IWorkContext> workContext)
            : base(authorizationService, repository)
        {
            this.membershipService = membershipService;
            this.postTagRepository = postTagRepository;
            this.workContext = workContext;
        }

        public override async Task<IActionResult> Post([FromBody] BlogPost entity)
        {
            int tenantId = GetTenantId();
            entity.TenantId = tenantId;

            if (!await CanModifyEntityAsync(entity))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            entity.DateCreatedUtc = DateTime.UtcNow;
            entity.UserId = workContext.Value.CurrentUser.Id;
            entity.FullDescription = MediaHelper.EnsureCorrectUrls(entity.FullDescription);

            var tags = entity.Tags;
            entity.Tags = null;

            SetNewId(entity);

            OnBeforeSave(entity);
            await Repository.InsertAsync(entity);

            var result = Created(entity);

            if (!tags.IsNullOrEmpty())
            {
                var toInsert = tags.Select(x => new BlogPostTag
                {
                    PostId = entity.Id,
                    TagId = x.TagId
                });
                postTagRepository.Value.Insert(toInsert);
            }

            OnAfterSave(entity);

            return result;
        }

        public override async Task<IActionResult> Put([FromODataUri] Guid key, [FromBody] BlogPost entity)
        {
            var currentEntry = await Repository.FindOneAsync(entity.Id);
            entity.TenantId = currentEntry.TenantId;
            entity.UserId = currentEntry.UserId;
            entity.DateCreatedUtc = currentEntry.DateCreatedUtc;
            entity.FullDescription = MediaHelper.EnsureCorrectUrls(entity.FullDescription);
            var result = await base.Put(key, entity);

            if (!entity.Tags.IsNullOrEmpty())
            {
                var chosenTagIds = entity.Tags.Select(x => x.TagId);
                var existingTags = await postTagRepository.Value.FindAsync(x => x.PostId == entity.Id);
                var existingTagIds = existingTags.Select(x => x.TagId);

                var toDelete = existingTags.Where(x => !chosenTagIds.Contains(x.TagId));
                var toInsert = chosenTagIds.Where(x => !existingTagIds.Contains(x)).Select(x => new BlogPostTag
                {
                    PostId = entity.Id,
                    TagId = x
                });

                await postTagRepository.Value.DeleteAsync(toDelete);
                await postTagRepository.Value.InsertAsync(toInsert);
            }

            return result;
        }

        protected override Guid GetId(BlogPost entity) => entity.Id;

        protected override void SetNewId(BlogPost entity) => entity.Id = Guid.NewGuid();

        protected override string ReadPermission => CmsConstants.Policies.BlogRead;

        protected override string WritePermission => CmsConstants.Policies.BlogWrite;
    }
}