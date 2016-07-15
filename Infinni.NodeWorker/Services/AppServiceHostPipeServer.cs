using System;
using System.Configuration;
using System.IO;
using System.Threading;

namespace Infinni.NodeWorker.Services
{
    public class AppServiceHostPipeServer : IDisposable
    {
        public AppServiceHostPipeServer(AppServiceOptions options, IAppServiceHost serviceHost)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (serviceHost == null)
            {
                throw new ArgumentNullException(nameof(serviceHost));
            }

            _serviceChannel = CreateServerChannel(options);
            _serviceHost = serviceHost;
        }


        private readonly FileChannelServer _serviceChannel;
        private readonly IAppServiceHost _serviceHost;


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


        private FileChannelServer CreateServerChannel(AppServiceOptions options)
        {
            FileChannelServer serviceChannel = null;

            try
            {
                var channelName = string.IsNullOrEmpty(options.PackageInstance)
                    ? $"{options.PackageId}.{options.PackageVersion}"
                    : $"{options.PackageId}.{options.PackageVersion}@{options.PackageInstance}";

                var channelDirectory = GetChannelDirectory();

                serviceChannel = new FileChannelServer(channelName, directory: channelDirectory)
                    .Subscribe("GetStatus", GetStatusHandler())
                    .Subscribe("Start", StartHandler())
                    .Subscribe("Stop", StopHandler());
            }
            catch
            {
                serviceChannel?.Dispose();

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

                OnStopServiceHost?.Invoke();

                return null;
            },
            (args, result) => InvokeStopServiceHost(),
            (args, error) => InvokeStopServiceHost());
        }

        private void InvokeStopServiceHost()
        {
            OnStopServiceHost?.Invoke();
        }

        private static string GetChannelDirectory()
        {
            return ConfigurationManager.AppSettings["WorkerServiceHostPipeDirectory"];
        }
    }
}