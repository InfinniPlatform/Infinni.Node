﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Logging;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Core.v3;
using NuGet.Resolver;
using NuGet.Versioning;

namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Менеджер для управления хранилищем NuGet-пакетов.
    /// </summary>
    public class NuGetPackageRepositoryManager : IPackageRepositoryManager
    {
        private static readonly NuGetFrameworkSorter NuGetFrameworkComparer = new NuGetFrameworkSorter();


        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="packagesPath">Путь к каталогу установки пакетов.</param>
        /// <param name="packageSources">Список источников пакетов.</param>
        /// <param name="logger">Сервис логирования.</param>
        public NuGetPackageRepositoryManager(string packagesPath, IEnumerable<string> packageSources, ILogger logger)
        {
            _packagesPath = packagesPath;
            _packageSources = packageSources;
            _logger = logger;
        }


        private readonly string _packagesPath;
        private readonly IEnumerable<string> _packageSources;
        private readonly ILogger _logger;


        /// <summary>
        /// Устанавливает пакет.
        /// </summary>
        /// <param name="packageId">ID пакета.</param>
        /// <param name="packageVersion">Версия пакета.</param>
        /// <param name="allowPrerelease">Разрешена ли установка предварительного релиза.</param>
        /// <returns>Содержимое установленного пакета.</returns>
        public async Task<PackageContent> InstallPackage(string packageId, string packageVersion = null, bool allowPrerelease = false)
        {
            NuGetVersion packageNuGetVersion = null;

            if (!string.IsNullOrWhiteSpace(packageVersion))
            {
                packageNuGetVersion = NuGetVersion.Parse(packageVersion);
            }

            // Конфигурационный файл NuGet.config по умолчанию
            var settings = new NuGet.Configuration.Settings(_packagesPath, "NuGet.config");

            // Фабрика источников пактов на основе конфигурационного файла
            var packageSourceProvider = new PackageSourceProvider(settings);

            // Добавление в фабрику источников пакетов дополнительных источников
            packageSourceProvider.SavePackageSources(_packageSources.Select(i => new PackageSource(i)));

            // Фабрика хранилищ пакетов на основе фабрики источников пакетов
            var packageRepositoryProvider = new CachingSourceProvider(packageSourceProvider);

            // Получение всех хранилищ пакетов на основе указанных источников
            var packageRepositories = packageRepositoryProvider.GetRepositories().ToList();

            // Определение возможности установки prerelease-версии пакетов
            allowPrerelease = allowPrerelease || (packageNuGetVersion != null && packageNuGetVersion.IsPrerelease);

            // Создание правил разрешения зависимостей при установке пакета
            var resolutionContext = new ResolutionContext(
                dependencyBehavior: DependencyBehavior.Lowest,
                includePrelease: allowPrerelease,
                includeUnlisted: true,
                versionConstraints: VersionConstraints.None);

            // Если версия пакета не указана, поиск последней версии
            if (packageNuGetVersion == null)
            {
                packageNuGetVersion = await NuGetPackageManager.GetLatestVersionAsync(
                    packageId,
                    NuGetFramework.AnyFramework,
                    resolutionContext,
                    packageRepositories,
                    _logger,
                    CancellationToken.None);

                if (packageNuGetVersion == null)
                {
                    throw new InvalidOperationException(string.Format(Properties.Resources.PackageNotFound, packageId));
                }
            }

            // Уникальный идентификатор версии пакета для установки
            var packageIdentity = new PackageIdentity(packageId, packageNuGetVersion);

            // Каталог для установки пакетов (каталог packages)
            NuGetProject folderProject = new FolderNuGetProject(_packagesPath);

            // Менеджер для управления пакетами
            var packageManager = new NuGetPackageManager(packageRepositoryProvider, settings, _packagesPath);

            // Правила установки пакетов
            var projectContext = new NuGetLoggerProjectContext(_logger)
            {
                PackageExtractionContext = new PackageExtractionContext
                {
                    PackageSaveMode = PackageSaveMode.Defaultv3
                }
            };

            // Определение порядка действий при установке пакета
            var installActions = (await packageManager.PreviewInstallPackageAsync(
                folderProject,
                packageIdentity,
                resolutionContext,
                projectContext,
                packageRepositories,
                Enumerable.Empty<SourceRepository>(),
                CancellationToken.None)).ToList();

            // Применение действий по установке пакета
            await packageManager.ExecuteNuGetProjectActionsAsync(
                folderProject,
                installActions,
                projectContext,
                CancellationToken.None);

            return GetPackageContent(packageIdentity, installActions.Select(i => i.PackageIdentity).ToList());
        }


        /// <summary>
        /// Возвращает содержимое пакета.
        /// </summary>
        /// <param name="packageIdentity">Идентификатор пакета.</param>
        /// <param name="packageDependencies">Список зависимостей пакета.</param>
        private PackageContent GetPackageContent(PackageIdentity packageIdentity, List<PackageIdentity> packageDependencies)
        {
            var packageContent = new PackageContent(packageIdentity, packageDependencies);

            var targetFramework = GetPackageLowestSupportedFramework(packageIdentity);

            if (packageDependencies != null)
            {
                foreach (var dependency in packageDependencies)
                {
                    FillPackageContent(packageContent, dependency, targetFramework, new DefaultCompatibilityProvider());
                }
            }

            return packageContent;
        }

        /// <summary>
        /// Добавляет в содержимое пакета совместимые версии файлов из указанного пакета.
        /// </summary>
        /// <param name="packageContent">Содержимое пакета.</param>
        /// <param name="packageIdentity">Идентификатор пакета.</param>
        /// <param name="targetFramework">Версия совместимого фреймворка.</param>
        /// <param name="compatibilityProvider">Провайдер для проверки совместимости фреймворков.</param>
        private void FillPackageContent(PackageContent packageContent,
                                        PackageIdentity packageIdentity,
                                        NuGetFramework targetFramework,
                                        IFrameworkCompatibilityProvider compatibilityProvider)
        {
            var packagePath = GetPackagePath(packageIdentity);

            using (var reader = new PackageFolderReader(packagePath))
            {
                // Из пакета выбираются файлы с TargetFramework, который
                // является наиболее новым и совместимым с указанным

                var libItems = reader.GetLibItems()
                                     .OrderByDescending(i => i.TargetFramework, NuGetFrameworkComparer)
                                     .FirstOrDefault(i => NuGetFrameworkComparer.Compare(i.TargetFramework, targetFramework) <= 0
                                                          && compatibilityProvider.IsCompatible(targetFramework, i.TargetFramework));

                var contentItems = reader.GetContentItems()
                                         .OrderByDescending(i => i.TargetFramework, NuGetFrameworkComparer)
                                         .FirstOrDefault(i => NuGetFrameworkComparer.Compare(i.TargetFramework, targetFramework) <= 0
                                                              && compatibilityProvider.IsCompatible(targetFramework, i.TargetFramework));

                if (libItems?.Items != null)
                {
                    foreach (var item in libItems.Items)
                    {
                        var installItem = GetPackageItem(packagePath, item, PackagingConstants.Folders.Lib, libItems.TargetFramework);
                        packageContent.Lib.Add(installItem);
                    }
                }

                if (contentItems?.Items != null)
                {
                    foreach (var item in contentItems.Items)
                    {
                        var installItem = GetPackageItem(packagePath, item, PackagingConstants.Folders.Content, contentItems.TargetFramework);
                        packageContent.Content.Add(installItem);
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает информацию о файле пакета для указанного источника и фреймворка.
        /// </summary>
        /// <param name="packagePath">Путь к каталогу пакета.</param>
        /// <param name="sourcePath">Путь к файлу в каталоге пакетов.</param>
        /// <param name="sourcePart">Каталог файла пакета ('lib', 'content' и т.д.).</param>
        /// <param name="sourceFramework">Версия фремворка файла пакета.</param>
        private static PackageFile GetPackageItem(string packagePath,
                                                  string sourcePath,
                                                  string sourcePart,
                                                  NuGetFramework sourceFramework)
        {
            // Путь файла в пакете обычно имеет вид 'lib/net45/some.dll'
            // или 'lib/some.dll' для NuGetFramework.AnyFramework

            sourcePath = sourcePath.Replace('/', Path.DirectorySeparatorChar);

            var installPath = sourcePath;

            // Определение части пути источника, которая указывает на NuGetFramework файла,
            // например, 'lib/net45/' или 'lib/' для NuGetFramework.AnyFramework

            var partFrameworkPath = sourcePart + Path.DirectorySeparatorChar;

            if (!Equals(sourceFramework, NuGetFramework.AnyFramework))
            {
                partFrameworkPath += sourceFramework.GetShortFolderName() + Path.DirectorySeparatorChar;
            }

            // Определение относительного пути для установки файла, например, для источника
            // 'lib/net45/some.dll' путь для установки будет 'some.dll', а для источника
            // 'lib/net45/en-US/resources.dll' путь для установки будет 'en-US/resources.dll'

            if (sourcePath.StartsWith(partFrameworkPath, StringComparison.OrdinalIgnoreCase))
            {
                installPath = sourcePath.Substring(partFrameworkPath.Length);
            }
            else if (!Equals(sourceFramework, NuGetFramework.AnyFramework))
            {
                // Обработка нестандартных путей, например, 'lib/net45-full/log4net.dll'

                var index = sourcePath.IndexOf(Path.DirectorySeparatorChar, sourcePart.Length + 1);

                if (index >= 0)
                {
                    installPath = sourcePath.Substring(index + 1);
                }
            }

            return new PackageFile(Path.Combine(packagePath, sourcePath), installPath);
        }

        /// <summary>
        /// Возвращает наименьшую поддерживаемую версию фреймворка для указанного пакета.
        /// </summary>
        /// <param name="packageIdentity">Идентификатор пакета.</param>
        private NuGetFramework GetPackageLowestSupportedFramework(PackageIdentity packageIdentity)
        {
            var packagePath = GetPackagePath(packageIdentity);

            using (var reader = new PackageFolderReader(packagePath))
            {
                // Выбираются только поддерживаемые TargetFramework, содержащие файлы в каталоге 'lib'

                return reader.GetLibItems()
                             .OrderBy(i => i.TargetFramework, NuGetFrameworkComparer)
                             .FirstOrDefault(i => !i.TargetFramework.IsUnsupported && i.Items != null && i.Items.Any())?.TargetFramework;
            }
        }

        /// <summary>
        /// Возвращает путь к каталогу указанного пакета.
        /// </summary>
        /// <param name="packageIdentity">Идентификатор пакета.</param>
        private string GetPackagePath(PackageIdentity packageIdentity)
        {
            // Путь установленного пакета обычно имеет вид 'packages/MyPackage.1.0.0'

            var nuGetVersion = packageIdentity.Version;

            var packagePath = Path.Combine(_packagesPath, $"{packageIdentity.Id}.{nuGetVersion}");

            if (!Directory.Exists(packagePath) && !nuGetVersion.IsPrerelease && nuGetVersion.Revision == 0)
            {
                packagePath += ".0";
            }

            return packagePath;
        }
    }
}