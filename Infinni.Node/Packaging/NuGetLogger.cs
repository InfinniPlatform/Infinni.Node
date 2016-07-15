using log4net;

using NuGet.Logging;

namespace Infinni.Node.Packaging
{
    public class NuGetLogger : ILogger
    {
        public NuGetLogger(ILog log)
        {
            _log = log;
        }


        private readonly ILog _log;


        public void LogDebug(string data)
        {
            _log.Debug(data);
        }

        public void LogVerbose(string data)
        {
            _log.Debug(data);
        }

        public void LogInformation(string data)
        {
            _log.Info(data);
        }

        public void LogMinimal(string data)
        {
            _log.Info(data);
        }

        public void LogWarning(string data)
        {
            _log.Warn(data);
        }

        public void LogError(string data)
        {
            _log.Error(data);
        }

        public void LogSummary(string data)
        {
            _log.Info(data);
        }
    }
}