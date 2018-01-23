using System;
using System.Linq;
using System.Threading.Tasks;

using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;
using Infinni.Node.Settings;
using log4net;

using NuGet.Packaging.Core;

namespace Infinni.Node.CommandHandlers
{
    public class InstallCommandHandler : CommandHandlerBase<InstallCommandOptions>
    {
        private const string InfinniPlatform = "InfinniPlatform";
        private const string InfinniPlatformSdk = "InfinniPlatform.Sdk";
        private const string InfinniPlatformServiceHost = "Infinni.NodeWorker";

        private const string AppExtensionConfig = "AppExtension.json";
        private const string AppCommonConfig = "AppCommon.json";
        private const string AppLogConfig = "AppLog.config";

        private const string AppDirectory = "app";
        private const string PlatformDirectory = "platform";


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
            CommonHelper.CheckAdministrativePrivileges();

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
                .Stage(Resources.InstallCommandHandler_InstallServiceHostPackage, InstallServiceHostPackage)
                .Stage(Resources.InstallCommandHandler_CopyAppFiles, CopyAppFiles)
                .Stage(Resources.InstallCommandHandler_InstallAppService, InstallAppService);

            await commandTransaction.Execute(commandContext);
        }


        private async Task InstallAppPackage(InstallCommandContext context)
        {
            var appPackageId = context.CommandOptions.Id;
            var appPackageVersion = context.CommandOptions.Version;

            if (!string.IsNullOrEmpty(appPackageVersion))
            {
                var appInstallation = _installDirectory.Create(appPackageId, appPackageVersion, context.CommandOptions.Instance);

                if (appInstallation.Directory.Exists)
                {
                    throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_AppAlreadyInstalled, CommonHelper.GetAppName(appPackageId, appPackageVersion, context.CommandOptions.Instance)));
                }
            }

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
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_AppPackageNotFound, CommonHelper.GetAppName(appPackageId, appPackageVersion)));
            }
        }

        private Task CheckAppInstallation(InstallCommandContext context)
        {
            var appPackageId = context.AppPackageContent.Identity.Id;
            var appPackageVersion = context.AppPackageContent.Identity.Version.ToString();

            context.AppInstallation = _installDirectory.Create(appPackageId, appPackageVersion, context.CommandOptions.Instance);

            if (context.AppInstallation.Directory.Exists)
            {
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_AppAlreadyInstalled, CommonHelper.GetAppName(appPackageId, appPackageVersion, context.CommandOptions.Instance)));
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
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_SdkDependencyNotFound, CommonHelper.GetAppName(appPackageId, appPackageVersion)));
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
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_PlatformPackageNotFound, CommonHelper.GetAppName(InfinniPlatform, sdkPackageVersion)));
            }
        }

        private static async Task InstallServiceHostPackage(InstallCommandContext context)
        {
            try
            {
                var version = AppSettings.GetValue("NodeWorkerVersion");

                context.ServiceHostPackageContent = await context.PackageRepository.InstallPackage(InfinniPlatformServiceHost, version, true);
            }
            catch (InvalidOperationException exception)
            {
                throw new CommandHandlerException(exception.Message, exception);
            }

            if (context.ServiceHostPackageContent == null)
            {
                throw new CommandHandlerException(string.Format(Resources.InstallCommandHandler_ServiceHostPackageNotFound, CommonHelper.GetAppName(InfinniPlatformServiceHost)));
            }
        }

        private Task CopyAppFiles(InstallCommandContext context)
        {
            _installDirectory.CopyFiles(context.AppInstallation, context.AppPackageContent, AppDirectory);
            _installDirectory.CopyFiles(context.AppInstallation, context.PlatformPackageContent, PlatformDirectory);
            _installDirectory.CopyFiles(context.AppInstallation, context.ServiceHostPackageContent, "");

            var appExtensionConfig = context.AppPackageContent.Lib.FirstOrDefault(i => string.Equals(i.InstallPath, AppExtensionConfig));

            if (appExtensionConfig != null)
            {
                _installDirectory.CopyFile(context.AppInstallation, appExtensionConfig, "");
            }

            var appCommonConfig = context.PlatformPackageContent.Lib.FirstOrDefault(i => string.Equals(i.InstallPath, AppCommonConfig));

            if (appCommonConfig != null)
            {
                _installDirectory.CopyFile(context.AppInstallation, appCommonConfig, "");
            }

            var appLogConfig = context.PlatformPackageContent.Lib.FirstOrDefault(i => string.Equals(i.InstallPath, AppLogConfig));

            if (appLogConfig != null)
            {
                _installDirectory.CopyFile(context.AppInstallation, appLogConfig, "");
            }

            return AsyncHelper.EmptyTask;
        }

        private async Task InstallAppService(InstallCommandContext context)
        {
            // Установка службы приложения
            await _appService.Install(context.AppInstallation);
        }


        class InstallCommandContext
        {
            public InstallCommandOptions CommandOptions;

            public IPackageRepositoryManager PackageRepository;

            public PackageContent AppPackageContent;

            public InstallDirectoryItem AppInstallation;

            public PackageIdentity SdkDependencyIdentity;

            public PackageContent PlatformPackageContent;

            public PackageContent ServiceHostPackageContent;
        }
    }
}