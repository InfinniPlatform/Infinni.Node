using log4net;

namespace Infinni.NodeWorker.Logging
{
    public static class Log
    {
        public static readonly ILog Default = LogManager.GetLogger("ILog");
    }
}