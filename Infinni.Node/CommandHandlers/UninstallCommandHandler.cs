using System.Threading.Tasks;

using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;

using log4net;

namespace Infinni.Node.CommandHandlers
{
    public class UninstallCommandHandler : CommandHandlerBase<UninstallCommandOptions>
    {
        public UninstallCommandHandler(IInstallDirectoryManager installDirectory,
                                       IAppServiceManager appService,
                                       ILog log)
        {
            _installDirectory = installDirectory;
            _appService = appService;
            _log = log;
        }


        private readonly IInstallDirectoryManager _installDirectory;
        private readonly IAppServiceManager _appService;
        private readonly ILog _log;


        public override async Task Handle(UninstallCommandOptions options)
        {
            CommonHelper.CheckAdministrativePrivileges();

            var commandContext = new UninstallCommandContext
            {
                CommandOptions = options
            };

            var commandTransaction = new CommandTransactionManager<UninstallCommandContext>(_log)
                .Stage(Resources.UninstallCommandHandler_FindAppInstallations, FindAppInstallations)
                .Stage(Resources.UninstallCommandHandler_UninstallAppServices, UninstallAppServices)
                .Stage(Resources.UninstallCommandHandler_DeleteAppFiles, DeleteAppFiles)
                ;

            await commandTransaction.Execute(commandContext);
        }


        private Task FindAppInstallations(UninstallCommandContext context)
        {
            context.AppInstallations = _installDirectory.GetItems(context.CommandOptions.Id, context.CommandOptions.Version, context.CommandOptions.Instance);

            if (context.AppInstallations == null || context.AppInstallations.Length <= 0)
            {
                throw new CommandHandlerException(Resources.UninstallCommandHandler_CanNotFindAnyApplicationsToUninstall);
            }

            return AsyncHelper.EmptyTask;
        }

        private async Task UninstallAppServices(UninstallCommandContext context)
        {
            foreach (var appInstallation in context.AppInstallations)
            {
                _log.InfoFormat(Resources.UninstallCommandHandler_StartUninstallAppService, appInstallation);

                // Удаление службы приложения
                await _appService.Uninstall(appInstallation);
            }
        }

        private Task DeleteAppFiles(UninstallCommandContext context)
        {
            foreach (var appInstallation in context.AppInstallations)
            {
                _log.InfoFormat(Resources.UninstallCommandHandler_StartDeleteAppFiles, appInstallation);

                _installDirectory.Delete(appInstallation);
            }

            return AsyncHelper.EmptyTask;
        }


        class UninstallCommandContext
        {
            public UninstallCommandOptions CommandOptions;

            public InstallDirectoryItem[] AppInstallations;
        }
    }
}