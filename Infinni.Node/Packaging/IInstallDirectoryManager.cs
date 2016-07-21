using System.Collections.Generic;

namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Менеджер по работе с каталогом установки приложения.
    /// </summary>
    public interface IInstallDirectoryManager
    {
        /// <summary>
        /// Создает сведения об установке приложения.
        /// </summary>
        /// <param name="packageId">ID пакета.</param>
        /// <param name="packageVersion">Версия пакета.</param>
        /// <param name="instance">Экземпляр пакета.</param>
        InstallDirectoryItem Create(string packageId, string packageVersion, string instance);

        /// <summary>
        /// Копирует файл пакета в каталог приложения.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        /// <param name="appFile">Файл пакета для копирования.</param>
        /// <param name="appLibPath">Путь к исполняемым файлам в каталоге.</param>
        void CopyFile(InstallDirectoryItem appInstallation, PackageFile appFile, string appLibPath);

        /// <summary>
        /// Копирует файлы пакета в каталог приложения.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        /// <param name="appPackage">Содержимое пакета для копирования.</param>
        /// <param name="appLibPath">Путь к исполняемым файлам в каталоге.</param>
        void CopyFiles(InstallDirectoryItem appInstallation, PackageContent appPackage, string appLibPath);

        /// <summary>
        /// Удаляет каталог.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        void Delete(InstallDirectoryItem appInstallation);

        /// <summary>
        /// Возвращает список установок.
        /// </summary>
        IEnumerable<InstallDirectoryItem> GetItems();
    }
}