using System;
using System.IO;
using System.Linq;

using Infinni.Node.CommandOptions;
using Infinni.Node.Logging;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Worker;

namespace Infinni.Node.CommandHandlers
{
	internal sealed class StopCommandHandler
	{
		public void Handle(CommandContext context, StopCommandOptions options)
		{
			CommandHandlerHelpers.CheckAdministrativePrivileges();

			var installDirectory = context.GetInstallDirectory();
			var workerService = context.GetWorkerService();

			var installItems = installDirectory.GetItems();

			if (!string.IsNullOrWhiteSpace(options.Id))
			{
				installItems = installItems.Where(i => string.Equals(i.PackageName.Id, options.Id.Trim(), StringComparison.OrdinalIgnoreCase));
			}

			if (!string.IsNullOrWhiteSpace(options.Version))
			{
				installItems = installItems.Where(i => string.Equals(i.PackageName.Version, options.Version.Trim(), StringComparison.OrdinalIgnoreCase));
			}

			if (!string.IsNullOrWhiteSpace(options.Instance))
			{
				installItems = installItems.Where(i => string.Equals(i.Instance, options.Instance.Trim(), StringComparison.OrdinalIgnoreCase));
			}

			var installItemsArray = installItems.ToArray();

			if (installItemsArray.Length <= 0)
			{
				Log.Default.InfoFormat(Resources.CanNotFindAnyApplicationsToStart);
			}
			else
			{
				foreach (var installItem in installItemsArray)
				{
					// Остановка сервиса рабочего процесса для приложения
					StopWorkerService(installDirectory, workerService, installItem, options.Timeout);
				}
			}
		}

		private static void StopWorkerService(IInstallDirectoryManager installDirectory, IWorkerServiceManager workerService, InstallDirectoryItem installItem, int? timeout)
		{
			Log.Default.InfoFormat(Resources.StoppingWorkerService, installItem.PackageName, installItem.Instance);

			try
			{
				// Рабочий каталог приложения
				var installDirectoryPath = installDirectory.GetPath(installItem.PackageName, installItem.Instance);
				var packageDirectory = Path.GetFullPath(installDirectoryPath);

				var serviceOptions = new WorkerServiceOptions
				{
					PackageId = installItem.PackageName.Id,
					PackageVersion = installItem.PackageName.Version,
					PackageInstance = installItem.Instance,
					PackageDirectory = packageDirectory,
					PackageTimeout = timeout
				};

				// Остановка рабочего процесса приложения
				workerService.Stop(serviceOptions).Wait();

				Log.Default.InfoFormat(Resources.StoppingWorkerServiceCompleted, installItem.PackageName, installItem.Instance);
			}
			catch (Exception error)
			{
				Log.Default.ErrorFormat(Resources.StoppingWorkerServiceCompletedWithError, installItem.PackageName, installItem.Instance, error);
			}
		}
	}
}