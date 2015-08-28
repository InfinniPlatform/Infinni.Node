using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infinni.Node.Packaging
{
	/// <summary>
	/// Информация о пакете.
	/// </summary>
	internal sealed class Package
	{
		public Package(PackageName name, string frameworkName, IList<PackageContent> contents)
		{
			Name = name;
			FrameworkName = frameworkName;
			Contents = new ReadOnlyCollection<PackageContent>(contents ?? new PackageContent[] { });
		}


		/// <summary>
		/// Наименование пакета.
		/// </summary>
		public readonly PackageName Name;

		/// <summary>
		/// Наменование версии .NET Framework.
		/// </summary>
		public readonly string FrameworkName;

		/// <summary>
		/// Список содержимого пакета.
		/// </summary>
		public readonly IReadOnlyCollection<PackageContent> Contents;
	}
}