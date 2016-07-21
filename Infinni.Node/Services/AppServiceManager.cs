using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Infinni.Node.Packaging;
using Infinni.Node.Properties;

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


        public Task Install(InstallDirectoryItem appInstallation)
        {
            return ExecuteWorkerService(WorkerServiceInstallVerb, appInstallation);
        }

        public Task Uninstall(InstallDirectoryItem appInstallation)
        {
            return ExecuteWorkerService(WorkerServiceUninstallVerb, appInstallation);
        }

        public Task Start(InstallDirectoryItem appInstallation, int? timeoutSeconds = null)
        {
            return ExecuteWorkerService(WorkerServiceStartVerb, appInstallation, timeoutSeconds);
        }

        public Task Stop(InstallDirectoryItem appInstallation, int? timeoutSeconds = null)
        {
            return ExecuteWorkerService(WorkerServiceStopVerb, appInstallation, timeoutSeconds);
        }

        public Task<object> GetStatus(InstallDirectoryItem appInstallation, int? timeoutSeconds = null)
        {
            return Task.FromResult<object>(Resources.NotImplemented);
        }


        private static Task ExecuteWorkerService(string commandVerb, InstallDirectoryItem appInstallation, int? timeoutSeconds = null)
        {
            var workerServiceFile = Path.Combine(appInstallation.Directory.FullName, WorkerServiceFile);
            var workerServiceArguments = BuildServiceCommand(commandVerb, appInstallation, timeoutSeconds);
            return MonoHelper.ExecuteProcess(workerServiceFile, workerServiceArguments);
        }

        private static string BuildServiceCommand(string commandVerb, InstallDirectoryItem appInstallation, int? timeoutSeconds)
        {
            var command = new StringBuilder(commandVerb);
            AddCommandOption(command, "packageId", appInstallation.PackageId);
            AddCommandOption(command, "packageVersion", appInstallation.PackageVersion);
            AddCommandOption(command, "packageInstance", appInstallation.Instance);
            AddCommandOption(command, "packageDirectory", appInstallation.Directory.FullName);
            AddCommandOption(command, "packageTimeout", timeoutSeconds);

            return command.ToString();
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