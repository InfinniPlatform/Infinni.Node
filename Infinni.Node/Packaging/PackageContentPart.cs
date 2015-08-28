using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infinni.Node.Packaging
{
	/// <summary>
	/// Раздел содержимого пакета.
	/// </summary>
	internal sealed class PackageContentPart
	{
		public PackageContentPart(string path, IList<string> files)
		{
			Path = path;
			Files = new ReadOnlyCollection<string>(files ?? new string[] { });
		}


		/// <summary>
		/// Путь к каталогу раздела.
		/// </summary>
		public readonly string Path;

		/// <summary>
		/// Список файлов раздела.
		/// </summary>
		public readonly IReadOnlyCollection<string> Files;
	}
}