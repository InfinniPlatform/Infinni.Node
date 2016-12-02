using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using Infinni.Node.Properties;
using Infinni.Node.Services;
using Infinni.Node.Settings;
using log4net;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

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
            _serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
        }


        private readonly IInstallDirectoryManager _installDirectory;
        private readonly IAppServiceManager _appService;
        private readonly ILog _log;
        private readonly JsonSerializer _serializer;

        public override async Task Handle(StatusCommandOptions options)
        {
            var commandContext = new StatusCommandContext
            {
                CommandOptions = options
            };

            var commandTransaction = new CommandTransactionManager<StatusCommandContext>(_log)
                .Stage(Resources.StatusCommandHandler_FindAppInstallations, FindAppInstallations)
                .Stage(Resources.StatusCommandHandler_GetStatusAppServices, GetStatusAppServices);

            await commandTransaction.Execute(commandContext);
        }

        private Task FindAppInstallations(StatusCommandContext context)
        {
            context.AppInstallations = _installDirectory.GetItems(context.CommandOptions.Id, context.CommandOptions.Version, context.CommandOptions.Instance);

            if (context.AppInstallations == null)
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

            var formatting = context.CommandOptions.Format
                                 ? Formatting.Indented
                                 : Formatting.None;

            var statusesJson = JArray.FromObject(statuses, _serializer)
                                     .ToString(formatting);

            _log.Info(statusesJson);
        }

        private async Task<object> GetStatusAppService(InstallDirectoryItem appInstallation, int? timeoutSeconds)
        {
            string error = null;
            var processInfo = new ProcessInfo
            {
                State = "Error while getting process information."
            };

            var imageUrl = GetImageUrl(appInstallation);

            try
            {
                processInfo = await _appService.GetProcessInfo(appInstallation, timeoutSeconds);
            }
            catch (AggregateException e)
            {
                error = (e.InnerExceptions.Count == 1)
                    ? e.InnerExceptions[0].Message
                    : e.Message;
            }
            catch (Exception e)
            {
                return new AppStatus(appInstallation, imageUrl, new ProcessInfo(), e.Message);
            }

            return new AppStatus(appInstallation, imageUrl, processInfo, error);
        }

        private static string GetImageUrl(InstallDirectoryItem appInstallation)
        {
            var packagesDirectoryName = AppSettings.GetValue("PackagesRepository");

            if (!string.IsNullOrEmpty(packagesDirectoryName))
            {
                var packageFullName = $"{appInstallation.PackageId}.{appInstallation.PackageVersion}";
                var packageDirectory = Path.Combine(packagesDirectoryName, packageFullName);

                var packageDirectoryInfo = new DirectoryInfo(packageDirectory);

                if (packageDirectoryInfo.Exists)
                {
                    var nuspecFilePath = Path.Combine(packageDirectoryInfo.FullName, $"{appInstallation.PackageId}.nuspec");

                    using (var stream = new FileStream(nuspecFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        var nuspec = XDocument.Load(stream);
                        var ns = nuspec.Root?.Name.Namespace;

                        return nuspec.Root?.Element(ns + "metadata")?.Element(ns + "iconUrl")?.Value;
                    }
                }
            }

            return string.Empty;
        }

        private class StatusCommandContext
        {
            public StatusCommandOptions CommandOptions;

            public InstallDirectoryItem[] AppInstallations;
        }
    }
}