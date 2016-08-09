using System;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.ProjectManagement;

namespace Infinni.Node.Packaging
{
    public class InfinniFolderNuGetProject : FolderNuGetProject
    {
        public InfinniFolderNuGetProject(string root) : base(root)
        {
            SetSupportedTargetFramework();
        }

        public InfinniFolderNuGetProject(string root, bool excludeVersion) : base(root, excludeVersion)
        {
            SetSupportedTargetFramework();
        }

        public InfinniFolderNuGetProject(string root, PackagePathResolver packagePathResolver) : base(root, packagePathResolver)
        {
            SetSupportedTargetFramework();
        }

        /// <summary>
        /// Устанавливает дефолтный фреймворк.
        /// </summary>
        /// <remarks>
        /// Устанавливает версию фреймворк, на основе которой работает Infinni.Node/Infinni.NodeWorker.
        /// TODO Поменять в случае перехода на другой фреймворк.
        /// </remarks>
        private void SetSupportedTargetFramework()
        {
            InternalMetadata[NuGetProjectMetadataKeys.TargetFramework] = FrameworkConstants.CommonFrameworks.Net452;
        }
    }
}