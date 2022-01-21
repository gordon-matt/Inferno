using Inferno.Tasks.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureInfernoOptions(this IServiceCollection services, IConfigurationRoot configuration)
        {
            //var infrastructureOptions = new InfernoInfrastructureOptions();
            //configuration.Bind(infrastructureOptions);
            //services.AddSingleton(infrastructureOptions);

            //var pluginOptions = new InfernoPluginOptions();
            //configuration.Bind(pluginOptions);
            //services.AddSingleton(pluginOptions);

            var tasksOptions = new InfernoTasksOptions();
            configuration.Bind(tasksOptions);
            services.AddSingleton(tasksOptions);

            //var webOptions = new InfernoWebOptions();
            //configuration.Bind(webOptions);
            //services.AddSingleton(webOptions);
        }
    }
}