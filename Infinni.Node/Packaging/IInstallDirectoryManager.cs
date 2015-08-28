using System.Collections.Generic;

namespace Infinni.Node.Packaging
{
	/// <summary>
	/// Менеджер по работе с каталогом установки.
	/// </summary>
	internal interface IInstallDirectoryManager
	{
		/// <summary>
		/// Возвращает путь к каталогу.
		/// </summary>
		/// <param name="packageName">Наименование пакета.</param>
		/// <param name="instance">Экземпляр пакета.</param>
		string GetPath(PackageName packageName, string instance = null);

		/// <summary>
		/// Проверяет существование каталога.
		/// </summary>
		/// <param name="packageName">Наименование пакета.</param>
		/// <param name="instance">Экземпляр пакета.</param>
		bool Exists(PackageName packageName, string instance = null);

		/// <summary>
		/// Создает каталог.
		/// </summary>
		/// <param name="packageName">Наименование пакета.</param>
		/// <param name="instance">Экземпляр пакета.</param>
		void Create(PackageName packageName, string instance = null);

		/// <summary>
		/// Удаляет каталог.
		/// </summary>
		/// <param name="packageName">Наименование пакета.</param>
		/// <param name="instance">Экземпляр пакета.</param>
		void Delete(PackageName packageName, string instance = null);

		/// <summary>
		/// Устанавливает файлы в каталог.
		/// </summary>
		/// <param name="package">Информация о пакете.</param>
		/// <param name="instance">Экземпляр пакета.</param>
		/// <param name="files">Список дополнительных файлов.</param>
		void Install(Package package, string instance = null, params string[] files);

		/// <summary>
		/// Возвращает список установок.
		/// </summary>
		IEnumerable<InstallDirectoryItem> GetItems();
	}
}