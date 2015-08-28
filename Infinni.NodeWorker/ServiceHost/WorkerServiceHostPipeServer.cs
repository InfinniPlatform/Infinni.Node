using System;
using System.IO;
using System.Threading;

namespace Infinni.NodeWorker.ServiceHost
{
	internal sealed class WorkerServiceHostPipeServer : IDisposable
	{
		public WorkerServiceHostPipeServer(WorkerServiceHostOptions options, IWorkerServiceHost serviceHost)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			if (serviceHost == null)
			{
				throw new ArgumentNullException("serviceHost");
			}

			_serviceChannel = CreateServerChannel(options);
			_serviceHost = serviceHost;
		}


		private readonly FileChannelServer _serviceChannel;
		private readonly IWorkerServiceHost _serviceHost;


		public Action OnStopServiceHost { get; set; }


		public void Start()
		{
			_serviceChannel.Start();
		}

		public void Stop()
		{
			_serviceChannel.Stop();
		}

		public void Dispose()
		{
			_serviceChannel.Dispose();
		}


		private FileChannelServer CreateServerChannel(WorkerServiceHostOptions options)
		{
			FileChannelServer serviceChannel = null;

			try
			{
				var channelName = string.IsNullOrEmpty(options.PackageInstance)
					? string.Format("{0}.{1}", options.PackageId, options.PackageVersion)
					: string.Format("{0}.{1}${2}", options.PackageId, options.PackageVersion, options.PackageInstance);

				serviceChannel = new FileChannelServer(channelName)
					.Subscribe("GetStatus", GetStatusHandler())
					.Subscribe("Start", StartHandler())
					.Subscribe("Stop", StopHandler());
			}
			catch
			{
				if (serviceChannel != null)
				{
					serviceChannel.Dispose();
				}

				throw;
			}

			return serviceChannel;
		}


		private IFileChannelHandler GetStatusHandler()
		{
			return new DelegateFileChannelHandler((dynamic args) =>
			{
				var status = _serviceHost.GetStatus();

				return status;
			});
		}

		private IFileChannelHandler StartHandler()
		{
			return new DelegateFileChannelHandler((dynamic args) =>
			{
				var timeout = (TimeSpan?)args.Timeout;

				_serviceHost.Start(timeout ?? Timeout.InfiniteTimeSpan);

				return null;
			});
		}

		private IFileChannelHandler StopHandler()
		{
			return new DelegateFileChannelHandler((dynamic args) =>
			{
				var timeout = (TimeSpan?)args.Timeout;

				_serviceHost.Stop(timeout ?? Timeout.InfiniteTimeSpan);

				if (OnStopServiceHost != null)
				{
					OnStopServiceHost();
				}

				return null;
			},
			(args, result) => InvokeStopServiceHost(),
			(args, error) => InvokeStopServiceHost());
		}

		private void InvokeStopServiceHost()
		{
			if (OnStopServiceHost != null)
			{
				OnStopServiceHost();
			}
		}
	}
}