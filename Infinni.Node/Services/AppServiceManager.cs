using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Infinni.NodeWorker.Services;

namespace Infinni.Node.Services
{
    /// <summary>
    /// Менеджер по работе с сервисами приложений.
    /// </summary>
    public class AppServiceManager : IAppServiceManager
    {
        private const string WorkerServiceFile = "Infinni.NodeWorker.exe";
        private const string WorkerServiceInstallVerb = "install";
        private const string WorkerServiceUninstallVerb = "uninstall";
        private const string WorkerServiceStartVerb = "start";
        private const string WorkerServiceStopVerb = "stop";


        public Task Install(AppServiceOptions options)
        {
            return ExecuteWorkerService(WorkerServiceInstallVerb, options);
        }

        public Task Uninstall(AppServiceOptions options)
        {
            return ExecuteWorkerService(WorkerServiceUninstallVerb, options);
        }

        public Task Start(AppServiceOptions options)
        {
            return ExecuteWorkerService(WorkerServiceStartVerb, options);
        }

        public Task Stop(AppServiceOptions options)
        {
            if (options.PackageTimeout == null)
            {
                return ExecuteWorkerService(WorkerServiceStopVerb, options);
            }

            return Task.Run(() =>
            {
                var timeout = TimeSpan.FromSeconds(options.PackageTimeout.Value);
                return InvokeService(options, c => c.Stop(timeout));
            });
        }

        public Task<object> GetStatus(AppServiceOptions options)
        {
            return InvokeService(options, c => c.GetStatus());
        }


        private static Task ExecuteWorkerService(string commandVerb, AppServiceOptions options)
        {
            var workerServiceFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WorkerServiceFile);
            var workerServiceArguments = BuildServiceCommand(commandVerb, options);
            return MonoHelper.ExecuteProcess(workerServiceFile, workerServiceArguments);
        }

        private static string BuildServiceCommand(string commandVerb, AppServiceOptions options)
        {
            var command = new StringBuilder(commandVerb);
            AddCommandOption(command, "packageId", options.PackageId);
            AddCommandOption(command, "packageVersion", options.PackageVersion);
            AddCommandOption(command, "packageInstance", options.PackageInstance);
            AddCommandOption(command, "packageConfig", options.PackageConfig);
            AddCommandOption(command, "packageDirectory", options.PackageDirectory);
            AddCommandOption(command, "packageTimeout", options.PackageTimeout);

            return command.ToString();
        }

        private static Task<T> InvokeService<T>(AppServiceOptions options, Func<AppServiceHostPipeClient, T> action)
        {
            return Task.Run(() =>
            {
                using (var client = new AppServiceHostPipeClient(options.PackageId, options.PackageVersion, options.PackageInstance))
                {
                    return action(client);
                }
            });
        }

        private static void AddCommandOption(StringBuilder command, string name, object value)
        {
            if (value != null && (!(value is string) || !string.IsNullOrWhiteSpace((string)value)))
            {
                command.AppendFormat(" -{0} \"{1}\"", name, value);
            }
        }
    }
}