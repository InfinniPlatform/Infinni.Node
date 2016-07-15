using System.Collections.Generic;

namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Менеджер по работе с каталогом установки.
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
        /// Устанавливает файлы в каталог.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        /// <param name="appPackages">Список пакетов для установки.</param>
        /// <param name="appFiles">Список дополнительных файлов.</param>
        void Install(InstallDirectoryItem appInstallation, IEnumerable<PackageContent> appPackages, params string[] appFiles);

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