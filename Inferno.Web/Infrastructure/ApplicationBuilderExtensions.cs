using Dependo;
using Dependo.Autofac;
using Inferno.Threading;
using Inferno.Web.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Inferno.Web.Infrastructure
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseInferno(this IApplicationBuilder app)
        {
            var webOptions = app.ApplicationServices.GetService<InfernoWebOptions>();
            if (!webOptions.IgnoreStartupTasks)
            {
                RunStartupTasks();
            }

            return app;
        }

        private static void RunStartupTasks()
        {
            var typeFinder = EngineContext.Current.Resolve<ITypeFinder>();

            if (!DataSettingsHelper.IsDatabaseInstalled)
            {
                return;
            }

            //find startup tasks provided by other assemblies
            var startupTasks = typeFinder.FindClassesOfType<IStartupTask>();

            //create and sort instances of startup tasks
            //we startup this interface even for not installed plugins.
            //otherwise, DbContext initializers won't run and a plugin installation won't work
            var instances = startupTasks
                .Select(startupTask => (IStartupTask)Activator.CreateInstance(startupTask))
                .OrderBy(startupTask => startupTask.Order);

            //execute tasks
            foreach (var task in instances)
            {
                AsyncHelper.RunSync(task.ExecuteAsync);
            }
        }
    }
}