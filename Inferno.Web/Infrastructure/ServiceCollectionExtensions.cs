using Inferno.Tasks.Configuration;
using Inferno.Web.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureInferno(this IServiceCollection services, IConfiguration configuration)
        {
            //var pluginOptions = new InfernoPluginOptions();
            //configuration.Bind(pluginOptions);
            //services.AddSingleton(pluginOptions);

            var tasksOptions = new InfernoTasksOptions();
            configuration.Bind(tasksOptions);
            services.AddSingleton(tasksOptions);

            var webOptions = new InfernoWebOptions();
            configuration.Bind(webOptions);
            services.AddSingleton(webOptions);
        }
    }
}