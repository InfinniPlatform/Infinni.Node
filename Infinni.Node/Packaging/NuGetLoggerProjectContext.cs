using System.Globalization;
using System.Xml.Linq;

using NuGet.Logging;
using NuGet.Packaging;
using NuGet.ProjectManagement;

namespace Infinni.Node.Packaging
{
    public class NuGetLoggerProjectContext : INuGetProjectContext
    {
        public NuGetLoggerProjectContext(ILogger logger)
        {
            _logger = logger;
        }

        private readonly ILogger _logger;

        public ExecutionContext ExecutionContext => null;

        public PackageExtractionContext PackageExtractionContext { get; set; }

        public XDocument OriginalPackagesConfig { get; set; }

        public ISourceControlManagerProvider SourceControlManagerProvider => null;

        public void Log(MessageLevel level, string message, params object[] args)
        {
            if (args.Length > 0)
            {
                message = string.Format(CultureInfo.CurrentCulture, message, args);
            }

            switch (level)
            {
                case MessageLevel.Debug:
                    _logger.LogDebug(message);
                    break;

                case MessageLevel.Info:
                    _logger.LogMinimal(message);
                    break;

                case MessageLevel.Warning:
                    _logger.LogWarning(message);
                    break;

                case MessageLevel.Error:
                    _logger.LogError(message);
                    break;
            }
        }

        public void ReportError(string message)
        {
            _logger.LogError(message);
        }

        public FileConflictAction ResolveFileConflict(string message)
        {
            return FileConflictAction.IgnoreAll;
        }

        public NuGetActionType ActionType { get; set; }
    }
}