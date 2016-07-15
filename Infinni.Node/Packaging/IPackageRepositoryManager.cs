using System.Threading.Tasks;

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
    }
}