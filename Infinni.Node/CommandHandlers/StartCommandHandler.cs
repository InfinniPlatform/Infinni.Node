﻿using System;
using System.Threading.Tasks;

using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;
using Infinni.NodeWorker.Services;

using log4net;

namespace Infinni.Node.CommandHandlers
{
    public class StartCommandHandler : CommandHandlerBase<StartCommandOptions>
    {
        public StartCommandHandler(IInstallDirectoryManager installDirectory,
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


        public override async Task Handle(StartCommandOptions options)
        {
            CommandHandlerHelpers.CheckAdministrativePrivileges();

            var commandContext = new StartCommandContext
            {
                CommandOptions = options
            };

            var commandTransaction = new CommandTransactionManager<StartCommandContext>(_log)
                .Stage(Resources.StartCommandHandler_FindAppInstallations, FindAppInstallations)
                .Stage(Resources.StartCommandHandler_StartAppServices, StartAppServices)
                ;

            await commandTransaction.Execute(commandContext);
        }


        private Task FindAppInstallations(StartCommandContext context)
        {
            context.AppInstallations = _installDirectory.GetItems(context.CommandOptions.Id, context.CommandOptions.Version, context.CommandOptions.Instance);

            if (context.AppInstallations == null || context.AppInstallations.Length <= 0)
            {
                throw new InvalidOperationException(Resources.StartCommandHandler_CanNotFindAnyApplicationsToStart);
            }

            return AsyncHelper.EmptyTask;
        }

        private async Task StartAppServices(StartCommandContext context)
        {
            foreach (var appInstallation in context.AppInstallations)
            {
                _log.InfoFormat(Resources.StartCommandHandler_StartAppService, appInstallation);

                var serviceOptions = new AppServiceOptions
                {
                    PackageId = appInstallation.PackageId,
                    PackageVersion = appInstallation.PackageVersion,
                    PackageInstance = appInstallation.Instance,
                    PackageDirectory = appInstallation.Directory.FullName
                };

                // Запуск рабочего процесса приложения
                await _appService.Start(serviceOptions);
            }
        }


        class StartCommandContext
        {
            public StartCommandOptions CommandOptions;

            public InstallDirectoryItem[] AppInstallations;
        }
    }
}