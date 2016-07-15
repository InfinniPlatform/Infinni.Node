using System;
using System.IO;

using Infinni.NodeWorker.Settings;

namespace Infinni.NodeWorker.Services
{
    public class AppServiceHostPipeClient : IDisposable
    {
        public AppServiceHostPipeClient(string packageId, string packageVersion, string packageInstance)
        {
            _serverChannel = new Lazy<FileChannelClient>(() => CreateServerChannel(packageId, packageVersion, packageInstance));
        }


        private readonly Lazy<FileChannelClient> _serverChannel;


        public object GetStatus()
        {
            return InvokeChannel(c => c.Invoke("GetStatus"));
        }

        public object Start(TimeSpan timeout)
        {
            return InvokeChannel(c => c.Invoke("Start", new { Timeout = timeout }), timeout);
        }

        public object Stop(TimeSpan timeout)
        {
            return InvokeChannel(c => c.Invoke("Stop", new { Timeout = timeout }), timeout);
        }

        public void Dispose()
        {
            if (_serverChannel.IsValueCreated)
            {
                _serverChannel.Value.Dispose();
            }
        }


        private static FileChannelClient CreateServerChannel(string packageId, string packageVersion, string packageInstance)
        {
            var channelName = CommonHelpers.GetAppName(packageId, packageVersion, packageInstance);
            var channelTimeout = Math.Max(AppSettings.GetValue("WorkerServiceHostPipeTimeout", 5), 5);
            var channelDirectory = AppSettings.GetValue("WorkerServiceHostPipeDirectory");

            return new FileChannelClient(channelName, directory: channelDirectory) { InvokeTimeout = TimeSpan.FromSeconds(channelTimeout) };
        }


        private object InvokeChannel(Func<FileChannelClient, object> action, TimeSpan? timeout = null)
        {
            var defaultTimeout = _serverChannel.Value.InvokeTimeout;

            if (timeout != null && timeout.Value > defaultTimeout)
            {
                _serverChannel.Value.InvokeTimeout = timeout.Value;
            }

            try
            {
                return action(_serverChannel.Value);
            }
            finally
            {
                _serverChannel.Value.InvokeTimeout = defaultTimeout;
            }
        }
    }
}