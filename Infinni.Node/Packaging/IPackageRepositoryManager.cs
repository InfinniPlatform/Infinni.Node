using System.Collections.Generic;
using System.Threading.Tasks;
using NuGet;

namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Менеджер для управления хранилищем пакетов.
    /// </summary>
    public interface IPackageRepositoryManager
    {
        /// <summary>
        /// Устанавливает пакет.
        /// </summary>
        /// <param name="packageId">ID пакета.</param>
        /// <param name="packageVersion">Версия пакета.</param>
        /// <param name="allowPrerelease">Разрешена ли установка предварительного релиза.</param>
        /// <returns>Содержимое установленного пакета.</returns>
        Task<PackageContent> InstallPackage(string packageId, string packageVersion = null, bool allowPrerelease = false);

        /// <summary>
        /// Возвращает список доступных в источниках пакетов по части ID.
        /// </summary>
        /// <param name="searchTerm">Часть ID пакета.</param>
        /// <param name="allowPrereleaseVersions">Разрешен ли поиск среди предрелизных версий.</param>
        Task<IEnumerable<IPackage>> FindAvailablePackages(string searchTerm, bool allowPrereleaseVersions);
    }
}