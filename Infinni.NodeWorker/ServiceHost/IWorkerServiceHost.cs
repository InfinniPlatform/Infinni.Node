using System;

namespace Infinni.NodeWorker.ServiceHost
{
	internal interface IWorkerServiceHost
	{
		string GetStatus();

		void Start(TimeSpan timeout);

		void Stop(TimeSpan timeout);
	}
}