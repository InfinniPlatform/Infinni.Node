using System;
using System.Collections.Generic;
using Infinni.Node.Settings;
using NuGet.Frameworks;
using NuGet.Logging;
using NuGet.Packaging;
using NuGet.ProjectManagement;

namespace Infinni.Node.Packaging
{
    public class InfinniFolderNuGetProject : FolderNuGetProject
    {
        private static readonly Dictionary<string, NuGetFramework> Frameworks = new Dictionary<string, NuGetFramework>
        {
            {"net452", FrameworkConstants.CommonFrameworks.Net452},
            {"net47", new NuGetFramework(".NETFramework", new Version(4, 7, 0, 0))}
        };

        public InfinniFolderNuGetProject(string root, ILogger logger) : base(root)
        {
            SetSupportedTargetFramework(logger);
        }

        public InfinniFolderNuGetProject(string root, bool excludeVersion, ILogger logger) : base(root, excludeVersion)
        {
            SetSupportedTargetFramework(logger);
        }

        public InfinniFolderNuGetProject(string root, PackagePathResolver packagePathResolver, ILogger logger) : base(root, packagePathResolver)
        {
            SetSupportedTargetFramework(logger);
        }

        /// <summary>
        /// Устанавливает дефолтный фреймворк.
        /// </summary>
        /// <remarks>
        /// Устанавливает версию фреймворк, на основе которой работает Infinni.Node/Infinni.NodeWorker.
        /// TODO Поменять в случае перехода на другой фреймворк.
        /// </remarks>
        private void SetSupportedTargetFramework(ILogger logger)
        {
            var version = AppSettings.GetValue("NetFrameworkVersion");

            try
            {
                InternalMetadata[NuGetProjectMetadataKeys.TargetFramework] = Frameworks[version];
            }
            catch (KeyNotFoundException)
            {
                logger.LogError($"Framework version '{version}'S is not supported. Supported values: {string.Join(", ", Frameworks.Keys)}.");
                throw;
            }
        }
    }
}