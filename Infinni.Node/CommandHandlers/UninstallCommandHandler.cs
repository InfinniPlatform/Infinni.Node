using System;
using System.IO;

using Infinni.Node.CommandOptions;
using Infinni.Node.Logging;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Worker;

namespace Infinni.Node.CommandHandlers
{
	internal sealed class UninstallCommandHandler
	{
		public void Handle(CommandContext context, UninstallCommandOptions options)
		{
			var installDirectory = context.GetInstallDirectory();
			var workerService = context.GetWorkerService();

			var installItems = installDirectory.GetItems(options.Id, options.Version, options.Instance);

			if (installItems.Length <= 0)
			{
				Log.Default.InfoFormat(Resources.CanNotFindAnyApplicationsToUninstall);
			}
			else
			{
				foreach (var installItem in installItems)
				{
					// Удаление сервиса рабочего процесса для приложения
					if (UninstallWorkerService(installDirectory, workerService, installItem))
					{
						// Удаление файлов пакета приложения из каталога установки
						DeletePackageFiles(installDirectory, installItem);
					}
				}
			}
		}

		private static bool UninstallWorkerService(IInstallDirectoryManager installDirectory, IWorkerServiceManager workerService, InstallDirectoryItem installItem)
		{
			Log.Default.InfoFormat(Resources.UninstallingWorkerService, installItem.PackageName, installItem.Instance);

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

				// Удаление рабочего процесса приложения
				workerService.Uninstall(serviceOptions).Wait();

				Log.Default.InfoFormat(Resources.UninstallingWorkerServiceCompleted, installItem.PackageName, installItem.Instance);

				return true;
			}
			catch (Exception error)
			{
				Log.Default.ErrorFormat(Resources.UninstallingWorkerServiceCompletedWithError, installItem.PackageName, installItem.Instance, error);

				return false;
			}
		}

		private static void DeletePackageFiles(IInstallDirectoryManager installDirectory, InstallDirectoryItem installItem)
		{
			Log.Default.InfoFormat(Resources.DeletingPackageFiles, installItem.PackageName, installItem.Instance);

			try
			{
				installDirectory.Delete(installItem.PackageName, installItem.Instance);

				Log.Default.InfoFormat(Resources.DeletingPackageFilesCompleted, installItem.PackageName, installItem.Instance);
			}
			catch (Exception error)
			{
				Log.Default.ErrorFormat(Resources.DeletingPackageFilesCompletedWithError, installItem.PackageName, installItem.Instance, error);
			}
		}
	}
}