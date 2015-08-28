using NuGet;

namespace Infinni.Node.Packaging
{
	internal sealed class NuGetLogger : ILogger
	{
		public static readonly ILogger Instance = new NuGetLogger();

		public FileConflictResolution ResolveFileConflict(string message)
		{
			return FileConflictResolution.Ignore;
		}

		public void Log(MessageLevel level, string message, params object[] args)
		{
			switch (level)
			{
				case MessageLevel.Debug:
					Logging.Log.Default.DebugFormat(message, args);
					break;
				case MessageLevel.Info:
					Logging.Log.Default.InfoFormat(message, args);
					break;
				case MessageLevel.Warning:
					Logging.Log.Default.WarnFormat(message, args);
					break;
				case MessageLevel.Error:
					Logging.Log.Default.ErrorFormat(message, args);
					break;
			}
		}
	}
}