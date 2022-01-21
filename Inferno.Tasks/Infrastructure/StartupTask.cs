//using System.Linq;
//using Dependo;
//using Extenso.Collections;
//using Extenso.Data.Entity;
//using Inferno.Infrastructure;
//using Inferno.Tasks.Configuration;
//using Inferno.Tasks.Entities;

//namespace Inferno.Tasks.Infrastructure
//{
//    public class StartupTask : IStartupTask
//    {
//        #region IStartupTask Members

//        public void Execute()
//        {
//            EnsureScheduledTasks();
//        }

//        private static void EnsureScheduledTasks()
//        {
//            var options = EngineContext.Current.Resolve<InfernoTasksOptions>();
//            if (options.ScheduledTasksEnabled)
//            {
//                var taskRepository = EngineContext.Current.Resolve<IRepository<ScheduledTask>>();
//                var allTasks = EngineContext.Current.ResolveAll<ITask>();
//                var allTaskNames = allTasks.Select(x => x.Name).ToList();
//                var installedTasks = taskRepository.Find();
//                var installedTaskNames = installedTasks.Select(x => x.Name).ToList();

//                var tasksToAdd = allTasks
//                    .Where(x => !installedTaskNames.Contains(x.Name))
//                    .Select(x => new ScheduledTask
//                    {
//                        Name = x.Name,
//                        Type = x.GetType().AssemblyQualifiedName,
//                        Seconds = x.DefaultInterval
//                    });

//                if (!tasksToAdd.IsNullOrEmpty())
//                {
//                    taskRepository.Insert(tasksToAdd);
//                }

//                var tasksToDelete = installedTasks.Where(x => !allTaskNames.Contains(x.Name));

//                if (!tasksToDelete.IsNullOrEmpty())
//                {
//                    taskRepository.Delete(tasksToDelete);
//                }
//            }
//        }

//        public int Order => 0;

//        #endregion IStartupTask Members
//    }
//}