namespace Infinni.Node.Packaging
{
	/// <summary>
	/// Хранилище пакетов.
	/// </summary>
	internal interface IPackageRepositoryManager
	{
		/// <summary>
		/// Возвращает наименование пакета.
		/// </summary>
		/// <param name="packageId">ID пакета.</param>
		/// <param name="packageVersion">Версия пакета.</param>
		/// <param name="allowPrereleaseVersions">Разрешена ли установка предварительного релиза.</param>
		PackageName GetPackageName(string packageId, string packageVersion = null, bool allowPrereleaseVersions = false);

		/// <summary>
		/// Устанавливает пакет.
		/// </summary>
		/// <param name="packageId">ID пакета.</param>
		/// <param name="packageVersion">Версия пакета.</param>
		/// <param name="ignoreDependencies">Нужно ли игнорировать зависимости пакета.</param>
		/// <param name="allowPrereleaseVersions">Разрешена ли установка предварительного релиза.</param>
		/// <returns>Список файлов установленного пакета.</returns>
		Package InstallPackage(string packageId, string packageVersion = null, bool ignoreDependencies = false, bool allowPrereleaseVersions = false);

		/// <summary>
		/// Удаляет пакет.
		/// </summary>
		/// <param name="packageId">ID пакета.</param>
		/// <param name="packageVersion">Версия пакета.</param>
		/// <param name="removeDependencies">Нужно ли удалять зависимости пакета.</param>
		void UninstallPackage(string packageId, string packageVersion = null, bool removeDependencies = false);
	}
}