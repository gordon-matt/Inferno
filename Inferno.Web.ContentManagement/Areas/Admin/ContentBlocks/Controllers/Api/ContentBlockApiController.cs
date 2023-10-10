﻿using Dependo;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Localization.Entities;
using Inferno.Localization.Services;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Controllers.Api
{
    //[Authorize(Roles = InfernoConstants.Roles.Administrators)]
    [Authorize]
    public class ContentBlockApiController : BaseODataController<ContentBlock, Guid>
    {
        private readonly Lazy<ILocalizablePropertyService> localizablePropertyService;

        public ContentBlockApiController(
            IAuthorizationService authorizationService,
            IRepository<ContentBlock> repository,
            Lazy<ILocalizablePropertyService> localizablePropertyService)
            : base(authorizationService, repository)
        {
            this.localizablePropertyService = localizablePropertyService;
        }

        protected override Guid GetId(ContentBlock entity) => entity.Id;

        protected override void SetNewId(ContentBlock entity) => entity.Id = Guid.NewGuid();

        public override async Task<IActionResult> Get(ODataQueryOptions<ContentBlock> options)
        {
            if (!await AuthorizeAsync(ReadPermission))
            {
                return Unauthorized();
            }

            //using (var connection = Service.OpenConnection())
            //{
            //    var results = options.ApplyTo(connection.Query(x => x.PageId == null));
            //    return await (results as IQueryable<ContentBlock>).ToHashSetAsync();
            //}

            // NOTE: Change due to: https://github.com/OData/WebApi/issues/1235
            var connection = GetDisposableConnection();
            var query = connection.Query(x => x.PageId == null);
            query = await ApplyMandatoryFilterAsync(query);
            var results = options.ApplyTo(query, IgnoreQueryOptions);

            var response = await Task.FromResult((results as IQueryable<ContentBlock>).ToHashSet());
            return Ok(response);
        }

        [HttpGet]
        public virtual async Task<IEnumerable<ContentBlock>> GetByPageId(
            [FromODataUri] Guid pageId,
            ODataQueryOptions<ContentBlock> options)
        {
            if (!await AuthorizeAsync(ReadPermission))
            {
                return Enumerable.Empty<ContentBlock>().AsQueryable();
            }

            var connection = GetDisposableConnection();
            var query = connection
                    .Query(x => x.PageId == pageId)
                    .OrderBy(x => x.ZoneId)
                    .ThenBy(x => x.Order);

            //var results = options.ApplyTo(query, AllowedQueryOptions);
            var results = options.ApplyTo(query);
            return await (results as IQueryable<ContentBlock>).ToHashSetAsync();
        }

        public override async Task<IActionResult> Put([FromODataUri] Guid key, [FromBody] ContentBlock entity)
        {
            var blockType = Type.GetType(entity.BlockType);
            var contentBlocks = EngineContext.Current.ResolveAll<IContentBlock>();
            var contentBlock = contentBlocks.First(x => x.GetType() == blockType);
            entity.BlockName = contentBlock.Name;
            return await base.Put(key, entity);
        }

        public override async Task<IActionResult> Post([FromBody] ContentBlock entity)
        {
            var blockType = Type.GetType(entity.BlockType);
            var contentBlocks = EngineContext.Current.ResolveAll<IContentBlock>();
            var contentBlock = contentBlocks.First(x => x.GetType() == blockType);
            entity.BlockName = contentBlock.Name;
            return await base.Post(entity);
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

            string entityType = typeof(ContentBlock).FullName;
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
            var entity = (ContentBlock)parameters["entity"];

            if (entity.Id == Guid.Empty)
            {
                return BadRequest();
            }
            string entityType = typeof(ContentBlock).FullName;
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

        protected override string ReadPermission => CmsConstants.Policies.ContentBlocksRead;

        protected override string WritePermission => CmsConstants.Policies.ContentBlocksWrite;
    }
}