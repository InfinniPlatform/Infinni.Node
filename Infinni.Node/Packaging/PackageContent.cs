using System.Collections.Generic;

using NuGet.Packaging.Core;

namespace Infinni.Node.Packaging
{
    /// <summary>
    /// Содержимое пакета.
    /// </summary>
    public class PackageContent
    {
        public PackageContent(PackageIdentity identity, IEnumerable<PackageIdentity> dependencies)
        {
            Identity = identity;
            Dependencies = dependencies;
            Lib = new List<PackageFile>();
            Content = new List<PackageFile>();
        }

        /// <summary>
        /// Идентификатор пакета.
        /// </summary>
        public PackageIdentity Identity { get; }

        /// <summary>
        /// Список зависимостей пакета.
        /// </summary>
        public IEnumerable<PackageIdentity> Dependencies { get; }

        /// <summary>
        /// Список файлов каталога 'lib'.
        /// </summary>
        public List<PackageFile> Lib { get; }

        /// <summary>
        /// Список файлов каталога 'content'.
        /// </summary>
        public List<PackageFile> Content { get; }
    }
}