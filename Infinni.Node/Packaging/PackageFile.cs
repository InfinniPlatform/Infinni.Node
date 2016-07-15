using System.Diagnostics;

namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Файл пакета.
    /// </summary>
    [DebuggerDisplay("{SourcePath}, {InstallPath}")]
    public class PackageFile
    {
        public PackageFile(string sourcePath, string installPath)
        {
            SourcePath = sourcePath;
            InstallPath = installPath;
        }

        /// <summary>
        /// Путь к файлу в каталоге пакетов.
        /// </summary>
        public string SourcePath { get; }

        /// <summary>
        /// Относительный путь для установки файла.
        /// </summary>
        public string InstallPath { get; }
    }
}