using Dependo;
using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Localize.Models;
using Localize.Properties;
using Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;

namespace Inferno.Web.ContentManagement.Areas.Admin.ContentBlocks.Controllers.Api;

[Authorize]
public class EntityTypeContentBlockApiController : BaseODataController<EntityTypeContentBlock, Guid>
{
    private readonly ILocalizablePropertyService localizableProperties;

    public EntityTypeContentBlockApiController(
        IAuthorizationService authorizationService,
        IRepository<EntityTypeContentBlock> repository,
        ILocalizablePropertyService localizableProperties)
        : base(authorizationService, repository)
    {
        this.localizableProperties = localizableProperties;
    }

    public override async Task<IActionResult> Post([FromBody] EntityTypeContentBlock entity, CancellationToken cancellationToken)
    {
        SetValues(entity);
        return await base.Post(entity, cancellationToken);
    }

    public override async Task<IActionResult> Put([FromODataUri] Guid key, [FromBody] EntityTypeContentBlock entity, CancellationToken cancellationToken)
    {
        SetValues(entity);
        return await base.Put(key, entity, cancellationToken);
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

        var localizedRecord = await localizableProperties.FindOneAsync(cultureCode, entityType, entityId, "BlockValues");
        if (localizedRecord.IsSuccess && localizedRecord.Value.Value is not null)
        {
            entity.BlockValues = localizedRecord.Value.Value;
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

        var saved = await localizableProperties.SaveAsync(new LocalizablePropertyForm
        {
            CultureCode = cultureCode,
            EntityType = entityType,
            EntityId = entityId,
            Property = "BlockValues",
            Value = entity.BlockValues,
        });

        return saved.IsSuccess ? Ok() : BadRequest();
    }

    protected override Guid GetId(EntityTypeContentBlock entity) => entity.Id;

    protected override void SetNewId(EntityTypeContentBlock entity) => entity.Id = Guid.NewGuid();

    protected override string ReadPermission => CmsConstants.Policies.ContentBlocksRead;

    protected override string WritePermission => CmsConstants.Policies.ContentBlocksWrite;

    private static void SetValues(EntityTypeContentBlock entity)
    {
        var blockType = Type.GetType(entity.BlockType);
        var contentBlocks = DependoResolver.Instance.ResolveAll<IContentBlock>();
        var contentBlock = contentBlocks.First(x => x.GetType() == blockType);
        entity.BlockName = contentBlock.Name;
    }
}