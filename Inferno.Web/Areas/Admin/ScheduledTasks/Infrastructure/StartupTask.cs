using Dependo;
using Extenso.Collections;
using Extenso.Data.Entity;
using Inferno.Tasks;
using Inferno.Tasks.Configuration;
using Inferno.Tasks.Entities;
using Inferno.Web.Infrastructure;
using Task = System.Threading.Tasks.Task;

namespace Inferno.Web.Areas.Admin.ScheduledTasks.Infrastructure
{
    /// <summary>
    /// Synchronizes the <see cref="ScheduledTask"/> database table with all <see cref="ITask"/>
    /// implementations discovered at startup, then starts the <see cref="TaskManager"/>.
    /// </summary>
    public class StartupTask : IStartupTask
    {
        public int Order => 100; // Run after the core startup task that creates the default tenant.

        public Task ExecuteAsync()
        {
            var options = DependoResolver.Instance.Resolve<InfernoTasksOptions>();
            if (options == null || !options.ScheduledTasksEnabled)
            {
                return Task.CompletedTask;
            }

            EnsureScheduledTasks();

            // Initialize the in-process scheduler so periodic tasks start running.
            TaskManager.Instance.Initialize();
            TaskManager.Instance.Start();

            return Task.CompletedTask;
        }

        private static void EnsureScheduledTasks()
        {
            var typeFinder = DependoResolver.Instance.Resolve<ITypeFinder>();
            var taskRepository = DependoResolver.Instance.Resolve<IRepository<ScheduledTask>>();

            var allTaskTypes = typeFinder.FindClassesOfType<ITask>().ToList();
            var allTaskInstances = allTaskTypes
                .Select(t =>
                {
                    try
                    {
                        return DependoResolver.Instance.ResolveUnregistered(t) as ITask;
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(t => t != null)
                .ToList();

            var allTaskNames = allTaskInstances.Select(x => x.Name).ToHashSet();
            List<ScheduledTask> installedTasks;
            using (var connection = taskRepository.OpenConnection())
            {
                installedTasks = connection.Query().ToList();
            }
            var installedTaskNames = installedTasks.Select(x => x.Name).ToHashSet();

            var tasksToAdd = allTaskInstances
                .Where(x => !installedTaskNames.Contains(x.Name))
                .Select(x => new ScheduledTask
                {
                    Name = x.Name,
                    Type = x.GetType().AssemblyQualifiedName,
                    Seconds = x.DefaultInterval,
                    Enabled = true
                })
                .ToList();

            if (!tasksToAdd.IsNullOrEmpty())
            {
                taskRepository.Insert(tasksToAdd);
            }

            var tasksToDelete = installedTasks
                .Where(x => !allTaskNames.Contains(x.Name))
                .ToList();

            if (!tasksToDelete.IsNullOrEmpty())
            {
                taskRepository.Delete(tasksToDelete);
            }
        }
    }
}
