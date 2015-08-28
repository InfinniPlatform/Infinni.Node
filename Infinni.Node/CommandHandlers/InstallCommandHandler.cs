using System;
using System.IO;

using Infinni.Node.CommandOptions;
using Infinni.Node.Logging;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Worker;

namespace Infinni.Node.CommandHandlers
{
	internal sealed class InstallCommandHandler
	{
		public void Handle(CommandContext context, InstallCommandOptions options)
		{
			var packageRepository = context.GetPackageRepository(options.Source);
			var installDirectory = context.GetInstallDirectory();
			var workerService = context.GetWorkerService();

			var packageName = packageRepository.GetPackageName(options.Id, options.Version, options.AllowPrereleaseVersions);

			if (installDirectory.Exists(packageName, options.Instance))
			{
				Log.Default.InfoFormat(Resources.PackageAlreadyInstalled, packageName, options.Instance);
			}
			else
			{
				// Установка пакета приложения в локальное хранилище пакетов
				var package = InstallPackage(packageRepository, packageName, options);

				// Копирование файлов пакета приложения в каталог установки
				var installDirectoryPath = CopyPackageFiles(installDirectory, package, options);

				// Установка сервиса рабочего процесса для приложения
				InstallWorkerService(workerService, installDirectoryPath, packageName, options);
			}
		}

		private static Package InstallPackage(IPackageRepositoryManager packageRepository, PackageName packageName, InstallCommandOptions options)
		{
			Log.Default.InfoFormat(Resources.InstallingPackage, packageName, options.Instance);

			try
			{
				var package = packageRepository.InstallPackage(options.Id, options.Version, options.IgnoreDependencies, options.AllowPrereleaseVersions);

				Log.Default.InfoFormat(Resources.InstallingPackageCompleted, packageName, options.Instance);

				return package;
			}
			catch (Exception error)
			{
				Log.Default.ErrorFormat(Resources.InstallingPackageCompletedWithError, packageName, options.Instance, error);

				throw;
			}
		}

		private static string CopyPackageFiles(IInstallDirectoryManager installDirectory, Package package, InstallCommandOptions options)
		{
			Log.Default.InfoFormat(Resources.CopyingPackageFiles, package.Name, options.Instance);

			try
			{
				installDirectory.Install(package, options.Instance, options.Config);

				Log.Default.InfoFormat(Resources.CopyingPackageFilesCompleted, package.Name, options.Instance);

				return installDirectory.GetPath(package.Name, options.Instance);
			}
			catch (Exception error)
			{
				Log.Default.ErrorFormat(Resources.CopyingPackageFilesCompletedWithError, package.Name, options.Instance, error);

				throw;
			}
		}

		private static void InstallWorkerService(IWorkerServiceManager workerService, string installDirectory, PackageName packageName, InstallCommandOptions options)
		{
			Log.Default.InfoFormat(Resources.InstallingWorkerService, packageName, options.Instance);

			try
			{
				// Рабочий каталог приложения
				var packageDirectory = Path.GetFullPath(installDirectory);

				// Файл конфигурации приложения
				var packageConfig = string.IsNullOrWhiteSpace(options.Config) ? string.Empty : Path.Combine(packageDirectory, Path.GetFileName(options.Config.Trim()));

				var serviceOptions = new WorkerServiceOptions
				{
					PackageId = packageName.Id,
					PackageVersion = packageName.Version,
					PackageInstance = options.Instance,
					PackageConfig = packageConfig,
					PackageDirectory = packageDirectory
				};

				// Установка рабочего процесса приложения
				workerService.Install(serviceOptions).Wait();

				Log.Default.InfoFormat(Resources.InstallingWorkerServiceCompleted, packageName, options.Instance);
			}
			catch (Exception error)
			{
				Log.Default.ErrorFormat(Resources.InstallingWorkerServiceCompletedWithError, packageName, options.Instance, error);

				throw;
			}
		}
	}
}