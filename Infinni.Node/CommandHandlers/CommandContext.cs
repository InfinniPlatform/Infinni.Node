using System.Linq;

using Infinni.Node.Packaging;
using Infinni.Node.Settings;
using Infinni.Node.Worker;

namespace Infinni.Node.CommandHandlers
{
	/// <summary>
	/// Контекст исполнения команды.
	/// </summary>
	internal sealed class CommandContext
	{
		private const string DefaultInstallDirectory = "install";
		private const string DefaultLocalRepository = "packages";
		private static readonly string[] DefaultSourceRepositories = { "https://www.nuget.org/api/v2/" };


		/// <summary>
		/// Возвращает каталог установки.
		/// </summary>
		public IInstallDirectoryManager GetInstallDirectory()
		{
			var rootInstallPath = GetConfigInstallDirectory();
			return new InstallDirectoryManager(rootInstallPath);
		}

		/// <summary>
		/// Возвращает хранилище пакетов.
		/// </summary>
		/// <param name="sourceRepositories">Список публичных источников пакетов.</param>
		public IPackageRepositoryManager GetPackageRepository(params string[] sourceRepositories)
		{
			var localRepository = GetConfigLocalRepository();
			var actualSourceRepositories = SelectSourceRepositories(sourceRepositories);
			return new NuGetPackageRepositoryManager(localRepository, actualSourceRepositories);
		}

		/// <summary>
		/// Возвращает сервис рабочих процессов.
		/// </summary>
		public IWorkerServiceManager GetWorkerService()
		{
			return new WorkerServiceManager();
		}


		private static string GetConfigInstallDirectory()
		{
			return AppSettings.GetValue("InstallDirectory", DefaultInstallDirectory);
		}

		private static string GetConfigLocalRepository()
		{
			return AppSettings.GetValue("LocalRepository", DefaultLocalRepository);
		}

		private static string[] GetConfigSourceRepositories()
		{
			return SelectNotEmptyValues(AppSettings.GetValues("SourceRepositories", DefaultSourceRepositories));
		}

		private static string[] SelectSourceRepositories(string[] sourceRepositories)
		{
			return SelectNotEmptyValues(sourceRepositories) ?? GetConfigSourceRepositories() ?? DefaultSourceRepositories;
		}

		private static string[] SelectNotEmptyValues(string[] values)
		{
			var notEmptyValues = (values != null) ? values.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).ToArray() : null;
			return (notEmptyValues != null && notEmptyValues.Length > 0) ? notEmptyValues : null;
		}
	}
}