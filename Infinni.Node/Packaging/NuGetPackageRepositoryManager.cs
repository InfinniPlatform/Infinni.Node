using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

using NuGet;

using Infinni.Node.Properties;

namespace Infinni.Node.Packaging
{
	/// <summary>
	/// Хранилище пакетов NuGet.
	/// </summary>
	internal sealed class NuGetPackageRepositoryManager : IPackageRepositoryManager
	{
		/// <summary>
		/// Конструктор.
		/// </summary>
		/// <param name="localRepository">Путь к каталогу загруженных NuGet-пакетов.</param>
		/// <param name="sourceRepositories">Список публичных источников NuGet-пакетов.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public NuGetPackageRepositoryManager(string localRepository, IEnumerable<string> sourceRepositories)
		{
			if (string.IsNullOrWhiteSpace(localRepository))
			{
				throw new ArgumentNullException("localRepository");
			}

			if (sourceRepositories == null)
			{
				throw new ArgumentNullException("sourceRepositories");
			}

			_localRepository = localRepository;
			_sourceRepositories = sourceRepositories;
		}


		private readonly string _localRepository;
		private readonly IEnumerable<string> _sourceRepositories;


		private PackageManager _nativePackageManager;

		private PackageManager GetNativePackageManager()
		{
			if (_nativePackageManager == null)
			{
				var repositoryFactory = new PackageRepositoryFactory();
				var sourceRepository = new AggregateRepository(repositoryFactory, _sourceRepositories, ignoreFailingRepositories: true);
				var nativePackageManager = new PackageManager(sourceRepository, _localRepository) { Logger = NuGetLogger.Instance };

				_nativePackageManager = nativePackageManager;
			}

			return _nativePackageManager;
		}


		public PackageName GetPackageName(string packageId, string packageVersion = null, bool allowPrereleaseVersions = false)
		{
			if (string.IsNullOrWhiteSpace(packageId))
			{
				throw new ArgumentNullException("packageId");
			}

			var nativePackageManager = GetNativePackageManager();

			// Шаг 1: Поиск устанавливаемого пакета в источнике
			var package = FindPackage(nativePackageManager, packageId, packageVersion, allowPrereleaseVersions);

			if (package == null)
			{
				throw new ArgumentException(string.Format(Resources.PackageNotFound, packageId, packageVersion));
			}

			return CreatePackageName(package);
		}

		public Package InstallPackage(string packageId, string packageVersion = null, bool ignoreDependencies = false, bool allowPrereleaseVersions = false)
		{
			if (string.IsNullOrWhiteSpace(packageId))
			{
				throw new ArgumentNullException("packageId");
			}

			var nativePackageManager = GetNativePackageManager();

			// Шаг 1: Поиск устанавливаемого пакета в источнике
			var package = FindPackage(nativePackageManager, packageId, packageVersion, allowPrereleaseVersions);

			if (package == null)
			{
				throw new ArgumentException(string.Format(Resources.PackageNotFound, packageId, packageVersion));
			}

			// Шаг 2: Загрузка и установка пакета в локальное хранилище
			nativePackageManager.InstallPackage(package, ignoreDependencies, allowPrereleaseVersions);

			// Шаг 3: Определение версии фреймворка для поиска зависимостей
			var targetFramework = GetTargetFramework(package);
			var targetFrameworkPath = (targetFramework != null) ? VersionUtility.GetShortFrameworkName(targetFramework) : null;

			// Шаг 4: Определение всех пакетов, от которых зависит устанавливаемый
			var packageDependencies = ignoreDependencies ? new[] { package } : FindPackageDependencies(nativePackageManager, package, allowPrereleaseVersions, targetFramework);

			// Шаг 5: Выбор из найденных пакетов файлов, совместимых с версией фреймворка
			var packageContents = packageDependencies.Select(p => CreatePackageContent(nativePackageManager, targetFramework, p)).ToArray();

			// Шаг 6: Формирование информации об установленном пакете
			var packageInfo = new Package(CreatePackageName(package), targetFrameworkPath, packageContents);

			return packageInfo;
		}

		public void UninstallPackage(string packageId, string packageVersion = null, bool removeDependencies = false)
		{
			if (string.IsNullOrWhiteSpace(packageId))
			{
				throw new ArgumentNullException("packageId");
			}

			var nativePackageManager = GetNativePackageManager();

			var packageSemanticVersion = ParsePackageVersion(packageVersion);

			// Шаг 1: Удаление пакета из локального хранилища
			nativePackageManager.UninstallPackage(packageId, packageSemanticVersion, false, removeDependencies);
		}


		private static IPackage FindPackage(PackageManager nativePackageManager, string packageId, string packageVersion, bool allowPrereleaseVersions)
		{
			var packageSemanticVersion = ParsePackageVersion(packageVersion);

			// Сначала делается попытка поиска пакета локальном хранилище

			return PackageRepositoryExtensions.FindPackage(nativePackageManager.LocalRepository, packageId, packageSemanticVersion, allowPrereleaseVersions, allowUnlisted: true)
				   ?? PackageRepositoryExtensions.FindPackage(nativePackageManager.SourceRepository, packageId, packageSemanticVersion, allowPrereleaseVersions, allowUnlisted: true);
		}

		private static IEnumerable<IPackage> FindPackageDependencies(PackageManager nativePackageManager, IPackage package, bool allowPrereleaseVersions, FrameworkName targetFramework)
		{
			var packageDependencies = new Dictionary<IPackage, IPackage>(PackageEqualityComparer.IdAndVersion);
			FillPackageDependencies(nativePackageManager, package, allowPrereleaseVersions, targetFramework, packageDependencies);
			return packageDependencies.Keys;
		}

		private static void FillPackageDependencies(PackageManager nativePackageManager, IPackage package, bool allowPrereleaseVersions, FrameworkName targetFramework, Dictionary<IPackage, IPackage> dependencies)
		{
			if (package != null && !dependencies.ContainsKey(package))
			{
				// Список пакетов, от которых зависит текущий пакет
				var packageDependencies = PackageExtensions.GetCompatiblePackageDependencies(package, targetFramework);

				if (packageDependencies != null)
				{
					var packageRepository = nativePackageManager.LocalRepository;
					var dependencyVersion = nativePackageManager.DependencyVersion;
					var constraintProvider = NullConstraintProvider.Instance;

					foreach (var packageDependency in packageDependencies)
					{
						// Поиск зависимости в локальном хранилище (делается предположение, что зависимость уже установлена)
						var dependencyPackage = PackageRepositoryExtensions.ResolveDependency(packageRepository, packageDependency, constraintProvider, allowPrereleaseVersions, false, dependencyVersion);

						// Рекурсивный вызов для найденной зависимости
						FillPackageDependencies(nativePackageManager, dependencyPackage, allowPrereleaseVersions, targetFramework, dependencies);
					}
				}

				dependencies[package] = package;
			}
		}

		private static PackageContent CreatePackageContent(PackageManager nativePackageManager, FrameworkName targetFramework, IPackage package)
		{
			var files = new Dictionary<string, List<string>>();
			var pathResolver = nativePackageManager.PathResolver;
			var packagePath = pathResolver.GetInstallPath(package);

			// Выбор из пакета файлов
			var packageFiles = GetCompatibleFles(targetFramework, package);

			if (packageFiles != null)
			{
				foreach (var file in packageFiles)
				{
					// Определение раздела

					var partPrefix = string.Empty;
					var partPrefixIndex = file.Path.IndexOf(Path.DirectorySeparatorChar);

					if (partPrefixIndex >= 0)
					{
						partPrefix = file.Path.Substring(0, partPrefixIndex);
					}

					List<string> partFiles;

					if (!files.TryGetValue(partPrefix, out partFiles))
					{
						partFiles = new List<string>();
						files.Add(partPrefix, partFiles);
					}

					// Добавление файла в раздел

					var filePath = Path.Combine(packagePath, file.Path);
					partFiles.Add(filePath);
				}
			}

			var packageParts = files.ToDictionary(i => i.Key, i => new PackageContentPart(Path.Combine(packagePath, i.Key), i.Value));

			return new PackageContent(CreatePackageName(package), packagePath, packageParts);
		}

		private static IEnumerable<IPackageFile> GetCompatibleFles(FrameworkName targetFramework, IPackage package)
		{
			IEnumerable<IPackageFile> result = null;

			var packageFiles = package.GetFiles();

			if (packageFiles != null)
			{
				var arrayPackageFiles = packageFiles.ToArray();

				// Файлы, совместимые с указанной версией фреймворка
				if (!VersionUtility.TryGetCompatibleItems(targetFramework, arrayPackageFiles, out result))
				{
					result = null;
				}

				// Файлы, у которых не указана версия фреймворка
				var anyFrameworkFiles = arrayPackageFiles.Where(i => i.SupportedFrameworks == null || i.SupportedFrameworks.IsEmpty());

				result = ((result == null) ? anyFrameworkFiles : result.Union(anyFrameworkFiles)).ToArray();
			}

			return result;
		}

		private static FrameworkName GetTargetFramework(IPackage package)
		{
			var frameworks = package.GetSupportedFrameworks();

			// Берется максимально поддерживаемая версия фреймворка (эту логику можно усовершенствовать)
			return (frameworks != null) ? frameworks.LastOrDefault(i => string.IsNullOrWhiteSpace(i.Profile)) : null;
		}

		private static SemanticVersion ParsePackageVersion(string packageVersion)
		{
			return string.IsNullOrWhiteSpace(packageVersion) ? null : new SemanticVersion(packageVersion);
		}

		private static PackageName CreatePackageName(IPackage package)
		{
			return new PackageName(package.Id, package.Version.ToNormalizedString());
		}
	}
}