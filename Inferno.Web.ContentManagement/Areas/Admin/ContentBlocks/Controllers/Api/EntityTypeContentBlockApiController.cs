using Dependo;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Localization.Entities;
using Inferno.Localization.Services;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Controllers.Api
{
    [Authorize]
    public class EntityTypeContentBlockApiController : BaseODataController<EntityTypeContentBlock, Guid>
    {
        private readonly Lazy<ILocalizablePropertyService> localizablePropertyService;

        public EntityTypeContentBlockApiController(
            IAuthorizationService authorizationService,
            IRepository<EntityTypeContentBlock> repository,
            Lazy<ILocalizablePropertyService> localizablePropertyService)
            : base(authorizationService, repository)
        {
            this.localizablePropertyService = localizablePropertyService;
        }

        public override async Task<IActionResult> Post([FromBody] EntityTypeContentBlock entity)
        {
            SetValues(entity);
            return await base.Post(entity);
        }

        public override async Task<IActionResult> Put([FromODataUri] Guid key, [FromBody] EntityTypeContentBlock entity)
        {
            SetValues(entity);
            return await base.Put(key, entity);
        }

        [HttpGet]
        public async Task<IActionResult> GetLocalized([FromODataUri] Guid id, [FromODataUri] string cultureCode)
        {
            if (!await AuthorizeAsync(ReadPermission))
            {
                return Unauthorized();
            }

            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var entity = await Repository.FindOneAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            string entityType = typeof(EntityTypeContentBlock).FullName;
            string entityId = entity.Id.ToString();

            var localizedRecord = await localizablePropertyService.Value.FindOneAsync(x =>
                x.CultureCode == cultureCode &&
                x.EntityType == entityType &&
                x.EntityId == entityId &&
                x.Property == "BlockValues");

            if (localizedRecord != null)
            {
                entity.BlockValues = localizedRecord.Value;
            }

            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLocalized([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string cultureCode = (string)parameters["cultureCode"];
            var entity = (EntityTypeContentBlock)parameters["entity"];

            if (entity.Id == Guid.Empty)
            {
                return BadRequest();
            }
            string entityType = typeof(EntityTypeContentBlock).FullName;
            string entityId = entity.Id.ToString();

            var localizedRecord = await localizablePropertyService.Value.FindOneAsync(x =>
                x.CultureCode == cultureCode &&
                x.EntityType == entityType &&
                x.EntityId == entityId &&
                x.Property == "BlockValues");

            if (localizedRecord == null)
            {
                localizedRecord = new LocalizableProperty
                {
                    CultureCode = cultureCode,
                    EntityType = entityType,
                    EntityId = entityId,
                    Property = "BlockValues",
                    Value = entity.BlockValues
                };
                await localizablePropertyService.Value.InsertAsync(localizedRecord);
                return Ok();
            }
            else
            {
                localizedRecord.Value = entity.BlockValues;
                await localizablePropertyService.Value.UpdateAsync(localizedRecord);
                return Ok();
            }
        }

        protected override Guid GetId(EntityTypeContentBlock entity) => entity.Id;

        protected override void SetNewId(EntityTypeContentBlock entity) => entity.Id = Guid.NewGuid();

        protected override string ReadPermission => CmsConstants.Policies.ContentBlocksRead;

        protected override string WritePermission => CmsConstants.Policies.ContentBlocksWrite;

        private static void SetValues(EntityTypeContentBlock entity)
        {
            var blockType = Type.GetType(entity.BlockType);
            var contentBlocks = EngineContext.Current.ResolveAll<IContentBlock>();
            var contentBlock = contentBlocks.First(x => x.GetType() == blockType);
            entity.BlockName = contentBlock.Name;
        }
    }
}