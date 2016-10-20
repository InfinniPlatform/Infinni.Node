using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Infinni.Node.Properties;
using Infinni.Node.Settings;

using log4net;

namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Менеджер по работе с каталогом установки приложения.
    /// </summary>
    public class InstallDirectoryManager : IInstallDirectoryManager
    {
        private const string DefaultInstallDirectory = "install";


        public InstallDirectoryManager(ILog log)
        {
            _rootInstallPath = AppSettings.GetValue("InstallDirectory", DefaultInstallDirectory);
            _log = log;
        }


        private readonly string _rootInstallPath;
        private readonly ILog _log;


        public InstallDirectoryItem Create(string packageId, string packageVersion, string instance)
        {
            var appDirectoryName = CommonHelper.GetAppName(packageId, packageVersion, instance);
            var appDirectoryPath = Path.Combine(_rootInstallPath, appDirectoryName);
            var appDirectory = new DirectoryInfo(appDirectoryPath);

            return new InstallDirectoryItem(packageId, packageVersion, instance, appDirectory);
        }

        public void Delete(InstallDirectoryItem appInstallation)
        {
            // Есть вероятность, что при удалении каталога установки его файлы
            // еще используются, поскольку служба останавливается асинхронно.
            // Пока более интеллектуального способа не найдено, производится
            // несколько попыток удаления каталога с интервалом в 1 секунду.

            Exception exception = null;

            // Максимальный таймаут равен 2 минуты (24 попытки по 5 секунд каждая)
            const int attempts = 24;
            const int timeout = 5000;

            for (var i = 0; i < attempts; i++)
            {
                if (exception != null)
                {
                    Thread.Sleep(timeout);
                }

                try
                {
                    if (appInstallation.Directory.Exists)
                    {
                        appInstallation.Directory.Delete(true);
                    }

                    return;
                }
                catch (Exception e)
                {
                    exception = e;

                    _log.InfoFormat(Resources.AttemptToDeletePackageDirectoryHasCompletedWithError, i + 1, appInstallation, e.Message);
                }
            }

            if (exception != null)
            {
                throw new InvalidOperationException(string.Format(Resources.CannotDeletePackageDirectory, appInstallation), exception);
            }
        }

        public void CopyFile(InstallDirectoryItem appInstallation, PackageFile appFile, string appLibPath)
        {
            var installPath = appInstallation.Directory.FullName;
            var installFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var destinationPath = Path.Combine(installPath, appLibPath, appFile.InstallPath);
            CopyFileWithOverwrite(appFile.SourcePath, destinationPath, installFiles);
        }

        public void CopyFiles(InstallDirectoryItem appInstallation, PackageContent appPackage, string appLibPath)
        {
            if (!appInstallation.Directory.Exists)
            {
                appInstallation.Directory.Create();
            }

            var installPath = appInstallation.Directory.FullName;
            var installFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Копирование файлов из каталога 'lib'
            foreach (var libFile in appPackage.Lib)
            {
                var destinationPath = Path.Combine(installPath, appLibPath, libFile.InstallPath);
                CopyFileWithOverwrite(libFile.SourcePath, destinationPath, installFiles);
            }

            // Копирование файлов из каталога 'content'
            foreach (var contentFile in appPackage.Content)
            {
                var destinationPath = Path.Combine(installPath, "content", contentFile.InstallPath);
                CopyFileWithOverwrite(contentFile.SourcePath, destinationPath, installFiles);
            }
        }

        private void CopyFileWithOverwrite(string sourcePath, string destinationPath, IDictionary<string, string> installFiles)
        {
            // Если файл уже был скопирован ранее
            if (installFiles.ContainsKey(destinationPath))
            {
                // Определяется источник копирования
                var previousSourcePath = installFiles[destinationPath];

                // Если источник не изменился, копирование не производится
                if (string.Equals(previousSourcePath, sourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Иначе выводится предупреждение о том, что файл будет переписан
                _log.WarnFormat(Resources.FileWillBeOverwritten, destinationPath, sourcePath, previousSourcePath);
            }

            // Добавление записи о факте копирования
            installFiles[destinationPath] = sourcePath;

            var destinationDir = Path.GetDirectoryName(destinationPath) ?? "";

            // Копирование файла из источника

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            File.Copy(sourcePath, destinationPath, true);
        }


        public IEnumerable<InstallDirectoryItem> GetItems()
        {
            if (!Directory.Exists(_rootInstallPath))
            {
                yield break;
            }

            var directories = Directory.EnumerateDirectories(_rootInstallPath);

            foreach (var path in directories)
            {
                var installDir = InstallDirectoryItem.Parse(path);

                if (installDir != null)
                {
                    yield return installDir;
                }
            }
        }
    }
}