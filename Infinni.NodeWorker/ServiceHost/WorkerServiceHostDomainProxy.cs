using System;
using System.Configuration;
using System.IO;

namespace Infinni.NodeWorker.ServiceHost
{
	internal sealed class WorkerServiceHostDomainProxy : IWorkerServiceHost, IDisposable
	{
		public WorkerServiceHostDomainProxy(WorkerServiceHostOptions options)
		{
			if (options == null)
			{
				throw new ArgumentNullException("options");
			}

			_domain = new Lazy<AppDomain>(() => CreateWorkerServiceDomain(options));
		}


		private readonly Lazy<AppDomain> _domain;


		private volatile bool _started;
		private readonly object _syncStarted = new object();


		public string GetStatus()
		{
			const string cGetStatusReturn = "GetStatus_return";

			_domain.Value.DoCallBack(() =>
			{
				var worker = GetWorkerServiceHost();
				var status = worker.GetStatus();
				SetDomainData(AppDomain.CurrentDomain, cGetStatusReturn, status);
			});

			return GetDomainData<string>(_domain.Value, cGetStatusReturn);
		}

		public void Start(TimeSpan timeout)
		{
			if (!_started)
			{
				lock (_syncStarted)
				{
					if (!_started)
					{
						const string cStartTimeout = "Start_timeout";

						SetDomainData(_domain.Value, cStartTimeout, timeout);

						_domain.Value.DoCallBack(() =>
						{
							var worker = GetWorkerServiceHost();
							var startTimeout = GetDomainData<TimeSpan>(AppDomain.CurrentDomain, cStartTimeout);
							worker.Start(startTimeout);
						});

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
						const string cStopTimeout = "Stop_timeout";

						SetDomainData(_domain.Value, cStopTimeout, timeout);

						_domain.Value.DoCallBack(() =>
						{
							var worker = GetWorkerServiceHost();
							var stopTimeout = GetDomainData<TimeSpan>(AppDomain.CurrentDomain, cStopTimeout);
							worker.Stop(stopTimeout);
						});

						_started = false;
					}
				}
			}
		}

		public void Dispose()
		{
			if (_domain.IsValueCreated)
			{
				DeleteWorkerServiceDomain(_domain.Value);
			}
		}


		private static AppDomain CreateWorkerServiceDomain(WorkerServiceHostOptions options)
		{
			var serviceHostContractName = GetServiceHostContractName();
			var domainFriendlyName = GetDomainFriendlyName(options.PackageId, options.PackageVersion, options.PackageInstance);
			var domainConfigurationFile = GetDomainConfigurationFile(options.PackageConfig);
			var domainApplicationBase = GetDomainApplicationBase(options.PackageDirectory);

			// Создание домена приложения
			var domain = AppDomain.CreateDomain(domainFriendlyName, null, new AppDomainSetup
			{
				ShadowCopyFiles = AppDomain.CurrentDomain.SetupInformation.ShadowCopyFiles,
				LoaderOptimization = LoaderOptimization.MultiDomainHost,
				ConfigurationFile = domainConfigurationFile,
				ApplicationBase = domainApplicationBase
			});

			DomainAssemblyResolver.Setup(domain);

			// Установка рабочего каталога
			SetCurrentDirectory(domain, domainApplicationBase);

			// Установка обработчика службы
			SetWorkerServiceHost(domain, serviceHostContractName);

			return domain;
		}

		private static void DeleteWorkerServiceDomain(AppDomain domain)
		{
			try
			{
				AppDomain.Unload(domain);
			}
			catch
			{
			}
		}


		private static string GetServiceHostContractName()
		{
			var value = ConfigurationManager.AppSettings["WorkerServiceHostContractName"];

			if (string.IsNullOrWhiteSpace(value))
			{
				value = "Infinni.NodeWorker.ServiceHost.IWorkerServiceHost";
			}

			return value.Trim();
		}

		private static string GetDomainFriendlyName(string packageId, string packageVersion, string packageInstance)
		{
			return string.IsNullOrWhiteSpace(packageInstance)
				? string.Format("{0}.{1}", packageId, packageVersion)
				: string.Format("{0}.{1}${2}", packageId, packageVersion, packageInstance);
		}

		private static string GetDomainConfigurationFile(string packageConfig)
		{
			return string.IsNullOrWhiteSpace(packageConfig)
				? AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
				: packageConfig;
		}

		private static string GetDomainApplicationBase(string packageDirectory)
		{
			return string.IsNullOrWhiteSpace(packageDirectory)
				? AppDomain.CurrentDomain.BaseDirectory
				: packageDirectory;
		}


		private static void SetCurrentDirectory(AppDomain domain, string currentDirectory)
		{
			const string cCurrentDirectory = "CurrentDirectory";

			SetDomainData(domain, cCurrentDirectory, currentDirectory);

			domain.DoCallBack(() =>
			{
				var currentDirectoryValue = GetDomainData<string>(AppDomain.CurrentDomain, cCurrentDirectory);
				Directory.SetCurrentDirectory(currentDirectoryValue);
			});
		}


		private static void SetWorkerServiceHost(AppDomain domain, string serviceHostContractName)
		{
			const string cServiceHostContractName = "ServiceHostContractName";

			SetDomainData(domain, cServiceHostContractName, serviceHostContractName);

			domain.DoCallBack(() =>
			{
				var contractName = GetDomainData<string>(AppDomain.CurrentDomain, cServiceHostContractName);
				var worker = new Lazy<IWorkerServiceHost>(() => new WorkerServiceHostImplementation(contractName));
				SetDomainData(AppDomain.CurrentDomain, "Instance", worker);
			});
		}

		private static IWorkerServiceHost GetWorkerServiceHost()
		{
			var domain = AppDomain.CurrentDomain;
			var worker = GetDomainData<Lazy<IWorkerServiceHost>>(domain, "Instance");
			return worker.Value;
		}


		private static void SetDomainData(AppDomain domain, string name, object data)
		{
			var dataKey = GetDomainDataKey(name);
			domain.SetData(dataKey, data);
		}

		private static T GetDomainData<T>(AppDomain domain, string name)
		{
			var dataKey = GetDomainDataKey(name);
			return (T)domain.GetData(dataKey);
		}


		private static string GetDomainDataKey(string name)
		{
			return string.Format("IWorkerServiceHost.{0}", name);
		}


		internal sealed class DomainAssemblyResolver
		{
			static DomainAssemblyResolver()
			{
				AppDomain.CurrentDomain.AssemblyResolve += (s, e) => typeof(DomainAssemblyResolver).Assembly;
			}

			public static void Setup(AppDomain domain)
			{
				domain.CreateInstanceFrom(typeof(DomainAssemblyResolver).Assembly.Location, typeof(DomainAssemblyResolver).FullName);
			}
		}
	}
}