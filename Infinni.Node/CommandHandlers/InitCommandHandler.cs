using System.Threading.Tasks;

using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;

using log4net;

namespace Infinni.Node.CommandHandlers
{
    public class InitCommandHandler : CommandHandlerBase<InitCommandOptions>
    {
        public InitCommandHandler(IInstallDirectoryManager installDirectory,
                                  IAppServiceManager appService,
                                  ILog log)
        {
            _installDirectory = installDirectory;
            _appService = appService;
            _log = log;
        }

        private readonly IAppServiceManager _appService;

        private readonly IInstallDirectoryManager _installDirectory;
        private readonly ILog _log;

        public override async Task Handle(InitCommandOptions options)
        {
            CommonHelper.CheckAdministrativePrivileges();

            var commandContext = new InitCommandContext
                                 {
                                     CommandOptions = options
                                 };

            var commandTransaction = new CommandTransactionManager<InitCommandContext>(_log)
                .Stage(Resources.InitCommandHandler_FindAppInstallations, FindAppInstallations)
                .Stage(Resources.InitCommandHandler_StartAppInitialization, StartAppInitialization);

            await commandTransaction.Execute(commandContext);
        }

        private Task FindAppInstallations(InitCommandContext context)
        {
            context.AppInstallations = _installDirectory.GetItems(context.CommandOptions.Id, context.CommandOptions.Version, context.CommandOptions.Instance);

            if ((context.AppInstallations == null) || (context.AppInstallations.Length <= 0))
            {
                throw new CommandHandlerException(Resources.InitCommandHandler_CanNotFindAnyApplicationsToStart);
            }

            return AsyncHelper.EmptyTask;
        }

        private async Task StartAppInitialization(InitCommandContext context)
        {
            foreach (var appInstallation in context.AppInstallations)
            {
                _log.InfoFormat(Resources.InitCommandHandler_StartInitialization, appInstallation);

                // Запуск рабочего процесса приложения
                await _appService.Init(appInstallation, context.CommandOptions.Timeout);
            }
        }


        private class InitCommandContext
        {
            public InstallDirectoryItem[] AppInstallations;
            public InitCommandOptions CommandOptions;
        }
    }
}