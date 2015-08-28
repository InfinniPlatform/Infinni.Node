using System;
using System.Collections.Generic;

using Infinni.NodeWorker.Logging;
using Infinni.NodeWorker.ServiceHost;

using Topshelf;

namespace Infinni.NodeWorker
{
	class Program
	{
		public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);


		static int Main()
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			Log.Default.Info(Environment.CommandLine);

			var result = HostFactory.Run(config =>
			{
				config.UseLog4Net();

				var parameters = config.SelectPlatform(i => i
					.AddStringParameter("packageId")
					.AddStringParameter("packageVersion")
					.AddStringParameter("packageInstance")
					.AddStringParameter("packageConfig")
					.AddStringParameter("packageDirectory")
					.AddStringParameter("packageTimeout"));

				var serviceOptions = ParseServiceOptions(parameters);
				var serviceTimeout = GetPackageTimeout(serviceOptions.PackageTimeout);

				config.SetStartTimeout(serviceTimeout);
				config.SetStopTimeout(serviceTimeout);

				config.Service<Tuple<IWorkerServiceHost, WorkerServiceHostPipeServer>>(s =>
				{
					s.ConstructUsing(hostSettings =>
					{
						var serviceHost = new WorkerServiceHostDomainProxy(serviceOptions);
						var serviceHostPipeServer = new WorkerServiceHostPipeServer(serviceOptions, serviceHost);
						var instance = new Tuple<IWorkerServiceHost, WorkerServiceHostPipeServer>(serviceHost, serviceHostPipeServer);

						return instance;
					});

					s.WhenStarted((instance, hostControl) =>
					{
						instance.Item1.Start(serviceTimeout);

						instance.Item2.OnStopServiceHost += hostControl.Stop;
						instance.Item2.Start();

						return true;
					});

					s.WhenStopped((instance, hostControl) =>
					{
						try
						{
							instance.Item1.Stop(serviceTimeout);
						}
						finally
						{
							instance.Item2.Stop();
						}

						return true;
					});
				});

				var serviceName = GetServiceName(serviceOptions);

				config.SetServiceName(serviceName);
				config.SetDisplayName(serviceName);
				config.SetDescription(serviceName);

				var serviceInstance = serviceOptions.PackageInstance;

				if (!string.IsNullOrWhiteSpace(serviceInstance))
				{
					config.SetInstanceName(serviceInstance);
				}
			});

			return (result == TopshelfExitCode.Ok) ? 0 : (int)result;
		}


		static WorkerServiceHostOptions ParseServiceOptions(IDictionary<string, object> parameters)
		{
			return new WorkerServiceHostOptions
			{
				PackageId = GetParameterValue(parameters, "packageId", "Infinni.NodeWorker"),
				PackageVersion = GetParameterValue(parameters, "packageVersion", "1.0.0"),
				PackageInstance = GetParameterValue(parameters, "packageInstance"),
				PackageConfig = GetParameterValue(parameters, "packageConfig"),
				PackageDirectory = GetParameterValue(parameters, "packageDirectory"),
				PackageTimeout = GetParameterValue(parameters, "packageTimeout")
			};
		}

		static string GetParameterValue(IDictionary<string, object> parameters, string parameterName, string defaultValue = null)
		{
			object value;

			if (parameters.TryGetValue(parameterName, out value) && !string.IsNullOrWhiteSpace(value as string))
			{
				return ((string)value).Trim();
			}

			return defaultValue;
		}

		static TimeSpan GetPackageTimeout(string value)
		{
			int packageTimeout;

			return (!string.IsNullOrWhiteSpace(value) && int.TryParse(value.Trim(), out packageTimeout) && packageTimeout >= 0)
				? TimeSpan.FromSeconds(packageTimeout)
				: DefaultTimeout;
		}

		static string GetServiceName(WorkerServiceHostOptions serviceOptions)
		{
			return string.Format(@"{0}.{1}", serviceOptions.PackageId, serviceOptions.PackageVersion);
		}

		static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Default.Fatal(e.ExceptionObject);
		}
	}
}