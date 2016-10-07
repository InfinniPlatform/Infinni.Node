using System.Threading.Tasks;
using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;
using log4net;

namespace Infinni.Node.CommandHandlers
{
    public class RestartCommandHandler : CommandHandlerBase<RestartCommandOptions>
    {
        private readonly IAppServiceManager _appService;


        private readonly IInstallDirectoryManager _installDirectory;
        private readonly ILog _log;

        public RestartCommandHandler(IInstallDirectoryManager installDirectory,
                                     IAppServiceManager appService,
                                     ILog log)
        {
            _installDirectory = installDirectory;
            _appService = appService;
            _log = log;
        }


        public override async Task Handle(RestartCommandOptions options)
        {
            CommonHelper.CheckAdministrativePrivileges();

            var commandContext = new RestartCommandContext
            {
                CommandOptions = options
            };

            var commandTransaction = new CommandTransactionManager<RestartCommandContext>(_log)
                .Stage(Resources.StartCommandHandler_FindAppInstallations, FindAppInstallations)
                .Stage(Resources.StopCommandHandler_StopAppServices, StopAppServices)
                .Stage(Resources.StartCommandHandler_StartAppServices, StartAppServices);

            await commandTransaction.Execute(commandContext);
        }


        private Task FindAppInstallations(RestartCommandContext context)
        {
            context.AppInstallations = _installDirectory.GetItems(context.CommandOptions.Id, context.CommandOptions.Version, context.CommandOptions.Instance);

            if ((context.AppInstallations == null) || (context.AppInstallations.Length <= 0))
            {
                throw new CommandHandlerException(Resources.StartCommandHandler_CanNotFindAnyApplicationsToStart);
            }

            return AsyncHelper.EmptyTask;
        }

        private async Task StartAppServices(RestartCommandContext context)
        {
            foreach (var appInstallation in context.AppInstallations)
            {
                _log.InfoFormat(Resources.StartCommandHandler_StartAppService, appInstallation);

                // Запуск рабочего процесса приложения
                await _appService.Start(appInstallation, context.CommandOptions.Timeout);
            }
        }

        private async Task StopAppServices(RestartCommandContext context)
        {
            foreach (var appInstallation in context.AppInstallations)
            {
                _log.InfoFormat(Resources.StopCommandHandler_StopAppService, appInstallation);

                // Остановка рабочего процесса приложения
                await _appService.Stop(appInstallation, context.CommandOptions.Timeout);
            }
        }


        private class RestartCommandContext
        {
            public InstallDirectoryItem[] AppInstallations;
            public RestartCommandOptions CommandOptions;
        }
    }
}