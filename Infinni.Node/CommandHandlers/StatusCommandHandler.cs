using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;

using log4net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infinni.Node.CommandHandlers
{
    public class StatusCommandHandler : CommandHandlerBase<StatusCommandOptions>
    {
        public StatusCommandHandler(IInstallDirectoryManager installDirectory,
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


        public override async Task Handle(StatusCommandOptions options)
        {
            var commandContext = new StatusCommandContext
            {
                CommandOptions = options
            };

            var commandTransaction = new CommandTransactionManager<StatusCommandContext>(_log)
                .Stage(Resources.StatusCommandHandler_FindAppInstallations, FindAppInstallations)
                .Stage(Resources.StatusCommandHandler_GetStatusAppServices, GetStatusAppServices)
                ;

            await commandTransaction.Execute(commandContext);
        }

        private Task FindAppInstallations(StatusCommandContext context)
        {
            context.AppInstallations = _installDirectory.GetItems(context.CommandOptions.Id, context.CommandOptions.Version, context.CommandOptions.Instance);

            if (context.AppInstallations == null || context.AppInstallations.Length <= 0)
            {
                throw new CommandHandlerException(Resources.StatusCommandHandler_CanNotFindAnyApplicationsToGetStatus);
            }

            return AsyncHelper.EmptyTask;
        }

        private async Task GetStatusAppServices(StatusCommandContext context)
        {
            var statuses = new List<object>();

            foreach (var appInstallation in context.AppInstallations)
            {
                var status = await GetStatusAppService(appInstallation, context.CommandOptions.Timeout);

                statuses.Add(status);
            }

            var jStatuses = JArray.FromObject(statuses).ToString(Formatting.None);

            _log.Info(jStatuses);
        }

        private async Task<object> GetStatusAppService(InstallDirectoryItem appInstallation, int? timeoutSeconds)
        {
            var status = new ProcessInfo
            {
                State = "Error while getting process information."
            };
            string error = null;

            try
            {
                status = await _appService.GetProcessInfo(appInstallation, timeoutSeconds);

            }
            catch (AggregateException e)
            {
                error = (e.InnerExceptions.Count == 1)
                    ? e.InnerExceptions[0].Message
                    : e.Message;
            }
            catch (Exception e)
            {
                error = e.Message;
                status = new ProcessInfo();
                return new AppStatus
                {
                    Id = appInstallation.PackageId,
                    Version = appInstallation.PackageVersion,
                    Instance = appInstallation.Instance,
                    ProcessInfo = status,
                    Error = error
                };
            }

            return new AppStatus
            {
                Id = appInstallation.PackageId,
                Version = appInstallation.PackageVersion,
                Instance = appInstallation.Instance,
                ProcessInfo = status,
                Error = error
            };
        }


        class StatusCommandContext
        {
            public StatusCommandOptions CommandOptions;

            public InstallDirectoryItem[] AppInstallations;
        }
    }
}