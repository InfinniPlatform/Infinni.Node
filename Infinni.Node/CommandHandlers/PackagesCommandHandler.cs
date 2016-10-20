using System.Linq;
using System.Threading.Tasks;
using Infinni.Node.CommandOptions;
using Infinni.Node.Packaging;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infinni.Node.CommandHandlers
{
    public class PackagesCommandHandler : CommandHandlerBase<PackagesCommandOptions>
    {
        private readonly ILog _log;
        private readonly IPackageRepositoryManagerFactory _packageRepositoryFactory;
        private readonly JsonSerializer _serializer;

        public PackagesCommandHandler(IPackageRepositoryManagerFactory packageRepositoryFactory, ILog log)
        {
            _packageRepositoryFactory = packageRepositoryFactory;
            _log = log;
            _serializer = new JsonSerializer {NullValueHandling = NullValueHandling.Ignore};
        }

        public override async Task Handle(PackagesCommandOptions options)
        {
            CommonHelper.CheckAdministrativePrivileges();

            var context = new PackagesCommandContext
            {
                CommandOptions = options,
                PackageRepository = _packageRepositoryFactory.Create()
            };

            var commandTransaction = new CommandTransactionManager<PackagesCommandContext>(_log)
                .Stage("List packages", FindPackages);

            await commandTransaction.Execute(context);
        }

        private async Task FindPackages(PackagesCommandContext context)
        {
            var packages = await context.PackageRepository.FindAvailablePackages(context.CommandOptions.Id, context.CommandOptions.AllowPrereleaseVersions);

            var formatting = context.CommandOptions.Format
                                 ? Formatting.Indented
                                 : Formatting.None;

            var statusesJson = JArray.FromObject(packages.Select(p => p.Id)
                                                         .Where(id => id.Contains(context.CommandOptions.Id))
                                                         .Distinct(),
                                                 _serializer)
                                     .ToString(formatting);

            _log.Info(statusesJson);
        }

        private class PackagesCommandContext
        {
            public PackagesCommandOptions CommandOptions;
            public IPackageRepositoryManager PackageRepository;
        }
    }
}