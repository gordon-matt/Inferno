using Inferno.Plugins;
using Inferno.Web.Areas.Admin.Plugins.Models;
using Inferno.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.Extensions.Logging;

namespace Inferno.Web.Areas.Admin.Plugins.Controllers.Api
{
    [Authorize]
    public class PluginApiController : ODataController
    {
        private readonly IPluginFinder pluginFinder;
        private readonly IAuthorizationService authorizationService;
        private readonly ILogger<PluginApiController> logger;

        public PluginApiController(
            IPluginFinder pluginFinder,
            IAuthorizationService authorizationService,
            ILoggerFactory loggerFactory)
        {
            this.pluginFinder = pluginFinder;
            this.authorizationService = authorizationService;
            logger = loggerFactory.CreateLogger<PluginApiController>();
        }

        public virtual async Task<IActionResult> Get(ODataQueryOptions<EdmPluginDescriptor> options)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.PluginsRead))
            {
                return Unauthorized();
            }

            var query = pluginFinder.GetPluginDescriptors(LoadPluginsMode.All)
                .Select(x => (EdmPluginDescriptor)x)
                .AsQueryable();

            return Ok(options.ApplyTo(query));
        }

        [EnableQuery]
        public virtual async Task<SingleResult<EdmPluginDescriptor>> Get([FromODataUri] string key)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.PluginsRead))
            {
                return SingleResult.Create(Enumerable.Empty<EdmPluginDescriptor>().AsQueryable());
            }

            string systemName = key.Replace('-', '.');
            var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);

            EdmPluginDescriptor entity = pluginDescriptor;
            return SingleResult.Create(new[] { entity }.AsQueryable());
        }

        public virtual async Task<IActionResult> Put([FromODataUri] string key, [FromBody] EdmPluginDescriptor entity)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.PluginsManage))
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string systemName = key.Replace('-', '.');
                var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName(systemName, LoadPluginsMode.All);

                if (pluginDescriptor == null)
                {
                    return NotFound();
                }

                pluginDescriptor.FriendlyName = entity.FriendlyName;
                pluginDescriptor.DisplayOrder = entity.DisplayOrder;
                pluginDescriptor.LimitedToTenants.Clear();
                if (entity.LimitedToTenants != null)
                {
                    foreach (int tenantId in entity.LimitedToTenants)
                    {
                        pluginDescriptor.LimitedToTenants.Add(tenantId);
                    }
                }

                PluginManager.SavePluginDescriptor(pluginDescriptor);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update plugin descriptor for key '{Key}'", key);
                return BadRequest(ex.Message);
            }

            return Updated(entity);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Install([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.PluginsManage))
            {
                return Unauthorized();
            }

            if (parameters == null || !parameters.TryGetValue("systemName", out object systemNameObj))
            {
                return BadRequest("Missing required parameter 'systemName'.");
            }

            string systemName = (systemNameObj as string)?.Replace('-', '.');
            if (string.IsNullOrEmpty(systemName))
            {
                return BadRequest("'systemName' must be a non-empty string.");
            }

            try
            {
                var pluginDescriptor = pluginFinder.GetPluginDescriptors(LoadPluginsMode.All)
                    .FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

                if (pluginDescriptor == null)
                {
                    return NotFound();
                }

                if (pluginDescriptor.Installed)
                {
                    return BadRequest("Plugin is already installed.");
                }

                pluginDescriptor.Instance().Install();
                pluginFinder.ReloadPlugins(pluginDescriptor);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to install plugin '{SystemName}'", systemName);
                return BadRequest(ex.GetBaseException().Message);
            }

            return Ok();
        }

        [HttpPost]
        public virtual async Task<IActionResult> Uninstall([FromBody] ODataActionParameters parameters)
        {
            if (!await AuthorizeAsync(InfernoWebPolicies.PluginsManage))
            {
                return Unauthorized();
            }

            if (parameters == null || !parameters.TryGetValue("systemName", out object systemNameObj))
            {
                return BadRequest("Missing required parameter 'systemName'.");
            }

            string systemName = (systemNameObj as string)?.Replace('-', '.');
            if (string.IsNullOrEmpty(systemName))
            {
                return BadRequest("'systemName' must be a non-empty string.");
            }

            try
            {
                var pluginDescriptor = pluginFinder.GetPluginDescriptors(LoadPluginsMode.All)
                    .FirstOrDefault(x => x.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

                if (pluginDescriptor == null)
                {
                    return NotFound();
                }

                if (!pluginDescriptor.Installed)
                {
                    return BadRequest("Plugin is not installed.");
                }

                pluginDescriptor.Instance().Uninstall();
                pluginFinder.ReloadPlugins(pluginDescriptor);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to uninstall plugin '{SystemName}'", systemName);
                return BadRequest(ex.GetBaseException().Message);
            }

            return Ok();
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
