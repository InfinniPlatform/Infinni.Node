using System;
using System.Threading.Tasks;

using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;
using Infinni.NodeWorker.Services;

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
            CommandHandlerHelpers.CheckAdministrativePrivileges();

            var commandContext = new StopCommandContext
            {
                CommandOptions = options
            };

            var commandTransaction = new CommandTransactionManager<StopCommandContext>(_log)
                .Stage(Resources.StopCommandHandler_FindAppInstallations, FindAppInstallations)
                .Stage(Resources.StopCommandHandler_StopAppServices, StopAppServices)
                ;

            await commandTransaction.Execute(commandContext);
        }


        private Task FindAppInstallations(StopCommandContext context)
        {
            context.AppInstallations = _installDirectory.GetItems(context.CommandOptions.Id, context.CommandOptions.Version, context.CommandOptions.Instance);

            if (context.AppInstallations == null || context.AppInstallations.Length <= 0)
            {
                throw new InvalidOperationException(Resources.StopCommandHandler_CanNotFindAnyApplicationsToStop);
            }

            return AsyncHelper.EmptyTask;
        }

        private async Task StopAppServices(StopCommandContext context)
        {
            foreach (var appInstallation in context.AppInstallations)
            {
                _log.InfoFormat(Resources.StartCommandHandler_StartAppService, appInstallation);

                var serviceOptions = new AppServiceOptions
                {
                    PackageId = appInstallation.PackageId,
                    PackageVersion = appInstallation.PackageVersion,
                    PackageInstance = appInstallation.Instance,
                    PackageDirectory = appInstallation.Directory.FullName,
                    PackageTimeout = context.CommandOptions.Timeout
                };

                // Остановка рабочего процесса приложения
                await _appService.Stop(serviceOptions);
            }
        }


        class StopCommandContext
        {
            public StopCommandOptions CommandOptions;

            public InstallDirectoryItem[] AppInstallations;
        }
    }
}