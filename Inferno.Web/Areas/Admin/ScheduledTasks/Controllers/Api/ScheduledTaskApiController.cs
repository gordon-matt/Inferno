using Extenso.AspNetCore.OData;
using Extenso.Data.Entity;
using Inferno.Tasks.Entities;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.Extensions.Logging;
using InfernoTask = Inferno.Tasks.Task;

namespace Inferno.Web.Areas.Admin.ScheduledTasks.Controllers.Api
{
    [Authorize]
    public class ScheduledTaskApiController : BaseODataController<ScheduledTask, int>
    {
        private readonly ILogger<ScheduledTaskApiController> logger;

        public ScheduledTaskApiController(
            IAuthorizationService authorizationService,
            IRepository<ScheduledTask> repository,
            ILogger<ScheduledTaskApiController> logger)
            : base(authorizationService, repository)
        {
            this.logger = logger;
        }

        protected override int GetId(ScheduledTask entity) => entity.Id;

        protected override void SetNewId(ScheduledTask entity)
        {
            // Database identity column will be assigned by EF Core.
        }

        public override async Task<IActionResult> Put(int key, [FromBody] ScheduledTask entity, CancellationToken cancellationToken)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            // Only allow editing the schedule-related fields. The Name/Type are owned by the
            // task discovery process at startup and must not be changed via the admin UI.
            var existing = await Repository.FindOneAsync(key);
            if (existing == null)
            {
                return NotFound();
            }

            existing.Seconds = entity.Seconds;
            existing.Enabled = entity.Enabled;
            existing.StopOnError = entity.StopOnError;

            return await base.Put(key, existing, cancellationToken);
        }

        [HttpPost]
        public virtual async Task<IActionResult> RunNow([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(WritePermission))
            {
                return Unauthorized();
            }

            if (parameters == null || !parameters.TryGetValue("taskId", out object taskIdObj))
            {
                return BadRequest("Missing required parameter 'taskId'.");
            }

            int taskId = Convert.ToInt32(taskIdObj);

            var scheduledTask = await Repository.FindOneAsync(taskId);
            if (scheduledTask == null)
            {
                return NotFound();
            }

            var task = new InfernoTask(scheduledTask)
            {
                // Force the task to run, even if currently disabled, when manually triggered.
                Enabled = true
            };

            try
            {
                task.Execute(throwException: true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while executing scheduled task '{TaskName}'.", scheduledTask.Name);
                return StatusCode(500, ex.Message);
            }

            return Ok();
        }

        protected override string ReadPermission => InfernoWebPolicies.ScheduledTasksRead;

        protected override string WritePermission => InfernoWebPolicies.ScheduledTasksWrite;
    }
}
