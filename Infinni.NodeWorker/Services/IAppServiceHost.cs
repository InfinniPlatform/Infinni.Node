using System;

namespace Infinni.NodeWorker.Services
{
    public interface IAppServiceHost
    {
        string GetStatus();

        void Start(TimeSpan timeout);

        void Stop(TimeSpan timeout);
    }
}