using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Infinni.Node.Packaging;

namespace Infinni.Node.Services
{
    public static class ProcessHelper
    {
        /// <summary>
        /// Возвращает статус процесса, соответствующего установленному приложению.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        public static Task<ProcessInfo> GetProcessInfo(InstallDirectoryItem appInstallation)
        {
            ProcessInfo processInfo;

            var process = Process.GetProcessesByName("Infinni.NodeWorker")
                                 .FirstOrDefault(p => Directory.GetParent(p.MainModule.FileName).Name == appInstallation.Directory.Name);

            if (process != null)
            {
                var versionInfo = process.MainModule.FileVersionInfo;

                processInfo = new ProcessInfo
                {
                    Id = process.Id,
                    State = "Running",
                    ModuleName = process.MainModule.ModuleName,
                    FileVersion = versionInfo.FileVersion,
                    ProductVersion = versionInfo.ProductVersion,
                    Language = versionInfo.Language,
                    IsPreRelease = versionInfo.IsPreRelease,
                    IsDebug = versionInfo.IsDebug,
                    ProductName = versionInfo.ProductName,
                    CompanyName = versionInfo.CompanyName,
                    LegalCopyright = versionInfo.LegalCopyright
                };
            }
            else
            {
                processInfo = new ProcessInfo
                {
                    State = "Stopped"
                };
            }

            return Task.FromResult(processInfo);
        }
    }


    /// <summary>
    /// Информация об установленном приложении.
    /// </summary>
    public struct AppStatus
    {
        public AppStatus(InstallDirectoryItem appInstallation, ProcessInfo processInfo, string error)
        {
            Id = appInstallation.PackageId;
            Version = appInstallation.PackageVersion;
            Instance = appInstallation.Instance;
            AppFullName = $"{appInstallation.PackageId}.{appInstallation.PackageVersion}@{appInstallation.Instance}".TrimEnd('@');
            ProcessInfo = processInfo;
            Error = error;
        }

        /// <summary>
        /// Идентификатор приложения.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Версия приложения.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Наименование экземпляра приложения.
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Полное наименование приложения.
        /// </summary>
        public string AppFullName { get; set; }

        /// <summary>
        /// Информация о запущенном процессе приложения.
        /// </summary>
        public ProcessInfo ProcessInfo { get; set; }

        /// <summary>
        /// Ошибки при получении статуса запущенного процесса.
        /// </summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Информация о процессе.
    /// </summary>
    public struct ProcessInfo
    {
        public int? Id { get; set; }
        public string State { get; set; }
        public string ModuleName { get; set; }
        public string FileVersion { get; set; }
        public string ProductVersion { get; set; }
        public string Language { get; set; }
        public bool? IsPreRelease { get; set; }
        public bool? IsDebug { get; set; }
        public string ProductName { get; set; }
        public string CompanyName { get; set; }
        public string LegalCopyright { get; set; }
    }
}