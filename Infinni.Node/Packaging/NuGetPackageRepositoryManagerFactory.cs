using System.Linq;

using Infinni.Node.Settings;

using NuGet.Logging;

namespace Infinni.Node.Packaging
{
    public class NuGetPackageRepositoryManagerFactory : IPackageRepositoryManagerFactory
    {
        private const string DefaultPackagesPath = "packages";

        private static readonly string[] DefaultPackageSources =
        {
            "https://api.nuget.org/v3/index.json",
            "https://www.nuget.org/api/v2/",
            "http://nuget.infinnity.ru/api/v2/"
        };


        public NuGetPackageRepositoryManagerFactory(ILogger logger)
        {
            _logger = logger;
        }


        private readonly ILogger _logger;


        public IPackageRepositoryManager Create(params string[] packageSources)
        {
            var packagesPath = AppSettings.GetValue("PackagesRepository", DefaultPackagesPath);

            packageSources = GetAllPackageSources(packageSources);

            return new NuGetPackageRepositoryManager(packagesPath, packageSources, _logger);
        }


        private static string[] GetAllPackageSources(string[] packageSources)
        {
            return DefaultPackageSources
                .Union(AppSettings.GetValues("PackageSources", new string[] { }))
                .Union(packageSources ?? new string[] { })
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.Trim())
                .ToArray();
        }
    }
}