using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;

namespace Infinni.NodeWorker.ServiceHost
{
	internal sealed class WorkerServiceHostImplementation : IWorkerServiceHost
	{
		public WorkerServiceHostImplementation(string serviceHostContractName)
		{
			if (string.IsNullOrEmpty(serviceHostContractName))
			{
				throw new ArgumentNullException("serviceHostContractName");
			}

			var directory = Directory.GetCurrentDirectory();
			var directoryCatalog = new DirectoryCatalog(directory);
			var compositionContainer = new CompositionContainer(directoryCatalog);

			_host = compositionContainer.GetExport<dynamic>(serviceHostContractName);
		}


		private readonly Lazy<dynamic> _host;


		private volatile bool _started;
		private readonly object _syncStarted = new object();


		public string GetStatus()
		{
			return _host.Value.GetStatus();
		}

		public void Start(TimeSpan timeout)
		{
			if (!_started)
			{
				lock (_syncStarted)
				{
					if (!_started)
					{
						_host.Value.Start(timeout);
						_started = true;
					}
				}
			}
		}

		public void Stop(TimeSpan timeout)
		{
			if (_started)
			{
				lock (_syncStarted)
				{
					if (_started)
					{
						_host.Value.Stop(timeout);
						_started = false;
					}
				}
			}
		}
	}
}