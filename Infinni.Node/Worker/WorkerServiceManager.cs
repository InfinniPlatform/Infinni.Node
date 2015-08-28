using System;
using System.Text;
using System.Threading.Tasks;

namespace Infinni.Node.Worker
{
	/// <summary>
	/// Менеджер по работе с сервисами рабочих процессов.
	/// </summary>
	internal sealed class WorkerServiceManager : IWorkerServiceManager
	{
		private const string WorkerServiceFile = "Infinni.NodeWorker.exe";
		private const string WorkerServiceInstallVerb = "install";
		private const string WorkerServiceUninstallVerb = "uninstall";
		private const string WorkerServiceStartVerb = "start";
		private const string WorkerServiceStopVerb = "stop";


		public Task Install(WorkerServiceOptions options)
		{
			var arguments = BuildServiceCommand(WorkerServiceInstallVerb, options);
			return MonoHelper.ExecuteProcess(WorkerServiceFile, arguments);
		}

		public Task Uninstall(WorkerServiceOptions options)
		{
			var arguments = BuildServiceCommand(WorkerServiceUninstallVerb, options);
			return MonoHelper.ExecuteProcess(WorkerServiceFile, arguments);
		}

		public Task Start(WorkerServiceOptions options)
		{
			var arguments = BuildServiceCommand(WorkerServiceStartVerb, options);
			return MonoHelper.ExecuteProcess(WorkerServiceFile, arguments);
		}

		public Task Stop(WorkerServiceOptions options)
		{
			if (options.PackageTimeout == null)
			{
				var arguments = BuildServiceCommand(WorkerServiceStopVerb, options);
				return MonoHelper.ExecuteProcess(WorkerServiceFile, arguments);
			}

			return Task.Run(() =>
			{
				var timeout = TimeSpan.FromSeconds(options.PackageTimeout.Value);
				return InvokeService(options, c => c.Stop(timeout));
			});
		}

		public Task<object> GetStatus(WorkerServiceOptions options)
		{
			return InvokeService(options, c => c.GetStatus());
		}


		private static string BuildServiceCommand(string commandVerb, WorkerServiceOptions options)
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

		private static Task<T> InvokeService<T>(WorkerServiceOptions options, Func<WorkerServiceHostPipeClient, T> action)
		{
			return Task.Run(() =>
			{
				using (var client = new WorkerServiceHostPipeClient(options.PackageId, options.PackageVersion, options.PackageInstance))
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