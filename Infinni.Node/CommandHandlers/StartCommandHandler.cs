using System;
using System.IO;

using Infinni.Node.CommandOptions;
using Infinni.Node.Logging;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Worker;

namespace Infinni.Node.CommandHandlers
{
	internal sealed class StartCommandHandler
	{
		public void Handle(CommandContext context, StartCommandOptions options)
		{
			var installDirectory = context.GetInstallDirectory();
			var workerService = context.GetWorkerService();

			var installItems = installDirectory.GetItems(options.Id, options.Version, options.Instance);

			if (installItems.Length <= 0)
			{
				Log.Default.InfoFormat(Resources.CanNotFindAnyApplicationsToStart);
			}
			else
			{
				foreach (var installItem in installItems)
				{
					// Запуск сервиса рабочего процесса для приложения
					StartWorkerService(installDirectory, workerService, installItem);
				}
			}
		}

		private static void StartWorkerService(IInstallDirectoryManager installDirectory, IWorkerServiceManager workerService, InstallDirectoryItem installItem)
		{
			Log.Default.InfoFormat(Resources.StartingWorkerService, installItem.PackageName, installItem.Instance);

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
					PackageDirectory = packageDirectory
				};

				// Запуск рабочего процесса приложения
				workerService.Start(serviceOptions).Wait();

				Log.Default.InfoFormat(Resources.StartingWorkerServiceCompleted, installItem.PackageName, installItem.Instance);
			}
			catch (Exception error)
			{
				Log.Default.ErrorFormat(Resources.StartingWorkerServiceCompletedWithError, installItem.PackageName, installItem.Instance, error);
			}
		}
	}
}