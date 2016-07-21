using System;
using System.ComponentModel.Composition.Hosting;

namespace Infinni.NodeWorker.Services
{
    public class AppServiceHost
    {
        public AppServiceHost()
        {
            // Поиск компонента для хостинга приложения
            _host = CreateComponent<dynamic>("InfinniPlatformServiceHost");
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


        private static Lazy<T> CreateComponent<T>(string contractName) where T : class
        {
            var aggregateCatalog = new DirectoryAssemblyCatalog();
            var compositionContainer = new CompositionContainer(aggregateCatalog);
            var lazyInstance = compositionContainer.GetExport<T>(contractName);
            return lazyInstance;
        }
    }
}