using System.IO;
using System.Text.RegularExpressions;

using Infinni.NodeWorker.Services;

namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Сведения об установке приложения.
    /// </summary>
    public class InstallDirectoryItem
    {
        private static readonly Regex DirectoryNameRegex = new Regex(@"^(?<Id>.*?)\.(?<Version>[0-9]+(\.[0-9]+(\.[0-9]+(\.[0-9]+){0,1}){0,1}){0,1}(\-.*?){0,1})(" + CommonHelpers.InstanceDelimiter + "(?<Instance>[^" + CommonHelpers.InstanceDelimiter + "]+)){0,1}$", RegexOptions.Compiled);


        public InstallDirectoryItem(string packageId, string packageVersion, string instance, DirectoryInfo directory)
        {
            PackageId = packageId;
            PackageVersion = packageVersion;
            Instance = instance;
            Directory = directory;
        }


        /// <summary>
        /// ID пакета приложения.
        /// </summary>
        public readonly string PackageId;

        /// <summary>
        /// Версия пакета приложения.
        /// </summary>
        public readonly string PackageVersion;

        /// <summary>
        /// Экземпляр приложения.
        /// </summary>
        public readonly string Instance;

        /// <summary>
        /// Каталог установки приложения.
        /// </summary>
        public readonly DirectoryInfo Directory;


        public static InstallDirectoryItem Parse(string directoryPath)
        {
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                var directoryName = Path.GetFileName(directoryPath);

                if (!string.IsNullOrWhiteSpace(directoryName))
                {
                    var directoryNameMatch = DirectoryNameRegex.Match(directoryName);

                    if (directoryNameMatch.Success)
                    {
                        var packageId = directoryNameMatch.Groups["Id"].Value;
                        var packageVersion = directoryNameMatch.Groups["Version"].Value;
                        var packageInstance = directoryNameMatch.Groups["Instance"].Value;

                        return new InstallDirectoryItem(packageId, packageVersion, packageInstance, new DirectoryInfo(directoryPath));
                    }
                }
            }

            return null;
        }


        public override string ToString()
        {
            return Directory.Name;
        }
    }
}