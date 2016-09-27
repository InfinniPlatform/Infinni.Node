using System.Threading.Tasks;

using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;

using log4net;

namespace Infinni.Node.CommandHandlers
{
    public class StopCommandHandler : CommandHandlerBase<StopCommandOptions>
    {
        public StopCommandHandler(IInstallDirectoryManager installDirectory,
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


        public override async Task Handle(StopCommandOptions options)
        {
            CommonHelper.CheckAdministrativePrivileges();

            var commandContext = new StopCommandContext
            {
                CommandOptions = options
            };

            var commandTransaction = new CommandTransactionManager<StopCommandContext>(_log)
                .Stage(Resources.StopCommandHandler_FindAppInstallations, FindAppInstallations)
                .Stage(string.Format(Resources.StopCommandHandler_StopAppServices,commandContext.CommandOptions.Instance), StopAppServices);

            await commandTransaction.Execute(commandContext);
        }


        private Task FindAppInstallations(StopCommandContext context)
        {
            context.AppInstallations = _installDirectory.GetItems(context.CommandOptions.Id, context.CommandOptions.Version, context.CommandOptions.Instance);

            if (context.AppInstallations == null || context.AppInstallations.Length <= 0)
            {
                throw new CommandHandlerException(Resources.StopCommandHandler_CanNotFindAnyApplicationsToStop);
            }

            return AsyncHelper.EmptyTask;
        }

        private async Task StopAppServices(StopCommandContext context)
        {
            foreach (var appInstallation in context.AppInstallations)
            {
                _log.InfoFormat(Resources.StopCommandHandler_StopAppService, appInstallation);

                // Остановка рабочего процесса приложения
                await _appService.Stop(appInstallation, context.CommandOptions.Timeout);
            }
        }


        class StopCommandContext
        {
            public StopCommandOptions CommandOptions;

            public InstallDirectoryItem[] AppInstallations;
        }
    }
}