using System.Diagnostics;

using log4net;

namespace Infinni.Node.Logging
{
	internal static class Log
	{
		static Log()
		{
			GlobalContext.Properties["pid"] = Process.GetCurrentProcess().Id;
		}

		public static readonly ILog Default = LogManager.GetLogger(typeof(Log));
	}
}