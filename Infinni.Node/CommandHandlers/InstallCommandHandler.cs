using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;
using Infinni.NodeWorker.Services;

using log4net;

using NuGet.Packaging.Core;

namespace Infinni.Node.CommandHandlers
{
    public class InstallCommandHandler : CommandHandlerBase<InstallCommandOptions>
    {
        private const string InfinniPlatform = "InfinniPlatform";
        private const string InfinniPlatformSdk = "InfinniPlatform.Sdk";


        public InstallCommandHandler(IPackageRepositoryManagerFactory packageRepositoryFactory,
                                     IInstallDirectoryManager installDirectory,
                                     IAppServiceManager appService,
                                     ILog log)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _installDirectory = installDirectory;
            _appService = appService;
            _log = log;
        }


        private readonly IPackageRepositoryManagerFactory _packageRepositoryFactory;
        private readonly IInstallDirectoryManager _installDirectory;
        private readonly IAppServiceManager _appService;
        private readonly ILog _log;


        public override async Task Handle(InstallCommandOptions options)
        {
            CommandHandlerHelpers.CheckAdministrativePrivileges();

            var commandContext = new InstallCommandContext
            {
                CommandOptions = options,
                PackageRepository = _packageRepositoryFactory.Create(options.Source)
            };

            var commandTransaction = new CommandTransactionManager<InstallCommandContext>(_log)
                .Stage(Resources.InstallCommandHandler_InstallAppPackage, InstallAppPackage)
                .Stage(Resources.InstallCommandHandler_CheckAppInstallation, CheckAppInstallation)
                .Stage(Resources.InstallCommandHandler_FindSdkDependency, FindSdkDependency)
                .Stage(Resources.InstallCommandHandler_InstallPlatformPackage, InstallPlatformPackage)
                .Stage(Resources.InstallCommandHandler_CopyAppFiles, CopyAppFiles)
                .Stage(Resources.InstallCommandHandler_InstallAppService, InstallAppService)
                ;

            await commandTransaction.Execute(commandContext);
        }


        private static async Task InstallAppPackage(InstallCommandContext context)
        {
            var appPackageId = context.CommandOptions.Id;
            var appPackageVersion = context.CommandOptions.Version;

            try
            {
                context.AppPackageContent = await context.PackageRepository.InstallPackage(appPackageId, appPackageVersion, context.CommandOptions.AllowPrereleaseVersions);
            }
            catch (InvalidOperationException exception)
            {
                throw new CommandHandlerException(exception.Message, exception);
            }

            if (context.AppPackageContent == null)
            {
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_AppPackageNotFound, CommonHelpers.GetAppName(appPackageId, appPackageVersion)));
            }
        }

        private Task CheckAppInstallation(InstallCommandContext context)
        {
            var appPackageId = context.AppPackageContent.Identity.Id;
            var appPackageVersion = context.AppPackageContent.Identity.Version.ToString();

            context.AppInstallation = _installDirectory.Create(appPackageId, appPackageVersion, context.CommandOptions.Instance);

            if (context.AppInstallation.Directory.Exists)
            {
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_AppAlreadyInstalled, CommonHelpers.GetAppName(appPackageId, appPackageVersion, context.CommandOptions.Instance)));
            }

            return AsyncHelper.EmptyTask;
        }

        private static Task FindSdkDependency(InstallCommandContext context)
        {
            var appPackageId = context.AppPackageContent.Identity.Id;
            var appPackageVersion = context.AppPackageContent.Identity.Version.ToString();

            context.SdkDependencyIdentity = context.AppPackageContent.Dependencies?.FirstOrDefault(i => string.Equals(i.Id, InfinniPlatformSdk, StringComparison.OrdinalIgnoreCase));

            if (context.SdkDependencyIdentity == null)
            {
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_SdkDependencyNotFound, CommonHelpers.GetAppName(appPackageId, appPackageVersion)));
            }

            return AsyncHelper.EmptyTask;
        }

        private static async Task InstallPlatformPackage(InstallCommandContext context)
        {
            var sdkPackageVersion = context.SdkDependencyIdentity.Version.ToString();

            try
            {
                context.PlatformPackageContent = await context.PackageRepository.InstallPackage(InfinniPlatform, sdkPackageVersion, context.CommandOptions.AllowPrereleaseVersions);
            }
            catch (InvalidOperationException exception)
            {
                throw new CommandHandlerException(exception.Message, exception);
            }

            if (context.PlatformPackageContent == null)
            {
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_PlatformPackageNotFound, CommonHelpers.GetAppName(InfinniPlatform, sdkPackageVersion)));
            }
        }

        private Task CopyAppFiles(InstallCommandContext context)
        {
            _installDirectory.Install(context.AppInstallation, new[] { context.PlatformPackageContent, context.AppPackageContent }, context.CommandOptions.Config);

            return AsyncHelper.EmptyTask;
        }

        private async Task InstallAppService(InstallCommandContext context)
        {
            var appPackageId = context.AppPackageContent.Identity.Id;
            var appPackageVersion = context.AppPackageContent.Identity.Version.ToString();

            // Рабочий каталог приложения
            var appDirectoryPath = context.AppInstallation.Directory.FullName;

            // Файл конфигурации приложения
            var appConfig = string.IsNullOrWhiteSpace(context.CommandOptions.Config) ? string.Empty : Path.Combine(appDirectoryPath, Path.GetFileName(context.CommandOptions.Config));

            var serviceOptions = new AppServiceOptions
            {
                PackageId = appPackageId,
                PackageVersion = appPackageVersion,
                PackageInstance = context.CommandOptions.Instance,
                PackageConfig = appConfig,
                PackageDirectory = appDirectoryPath
            };

            // Установка службы приложения
            await _appService.Install(serviceOptions);
        }


        class InstallCommandContext
        {
            public InstallCommandOptions CommandOptions;

            public IPackageRepositoryManager PackageRepository;

            public PackageContent AppPackageContent;

            public InstallDirectoryItem AppInstallation;

            public PackageIdentity SdkDependencyIdentity;

            public PackageContent PlatformPackageContent;
        }
    }
}