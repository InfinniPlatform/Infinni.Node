using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Infinni.Node.Packaging
{
    /// <summary>
	/// Содержимое пакета.
	/// </summary>
	[DebuggerDisplay("{Name}")]
	internal sealed class PackageContent
	{
		public PackageContent(PackageName name, string path, IDictionary<string, PackageContentPart> parts)
		{
			Name = name;
			Path = path;
			Parts = new ReadOnlyDictionary<string, PackageContentPart>(parts ?? new Dictionary<string, PackageContentPart>());
		}


		/// <summary>
		/// Наименование пакета.
		/// </summary>
		public readonly PackageName Name;

		/// <summary>
		/// Путь к каталогу пакета.
		/// </summary>
		public readonly string Path;

		/// <summary>
		/// Список разделов содержимого пакета.
		/// </summary>
		public readonly IReadOnlyDictionary<string, PackageContentPart> Parts;
	}
}