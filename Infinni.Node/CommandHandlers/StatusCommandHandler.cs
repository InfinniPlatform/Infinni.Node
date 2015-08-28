using System;
using System.Collections.Generic;

using Infinni.Node.CommandOptions;
using Infinni.Node.Logging;
using Infinni.Node.Packaging;
using Infinni.Node.Worker;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Infinni.Node.CommandHandlers
{
	internal sealed class StatusCommandHandler
	{
		public void Handle(CommandContext context, StatusCommandOptions options)
		{
			var installDirectory = context.GetInstallDirectory();
			var workerService = context.GetWorkerService();

			var statuses = new List<object>();
			var installItems = installDirectory.GetItems(options.Id, options.Version, options.Instance);

			if (installItems.Length > 0)
			{
				foreach (var installItem in installItems)
				{
					var status = GetStatusWorkerService(workerService, installItem);
					statuses.Add(status);
				}
			}

			var jStatuses = JArray.FromObject(statuses).ToString(Formatting.None);

			Log.Default.Info(jStatuses);
		}

		private static object GetStatusWorkerService(IWorkerServiceManager workerService, InstallDirectoryItem installItem)
		{
			object status = null;
			object error = null;

			var serviceOptions = new WorkerServiceOptions
			{
				PackageId = installItem.PackageName.Id,
				PackageVersion = installItem.PackageName.Version,
				PackageInstance = installItem.Instance
			};

			try
			{
				status = workerService.GetStatus(serviceOptions).Result;
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
			}

			return new
			{
				Id = installItem.PackageName.Id,
				Version = installItem.PackageName.Version,
				Instance = installItem.Instance,
				Status = status,
				Error = error
			};
		}
	}
}