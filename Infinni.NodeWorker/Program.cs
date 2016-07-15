using System;
using System.Collections.Generic;

using Infinni.NodeWorker.Logging;
using Infinni.NodeWorker.Services;

using Topshelf;

namespace Infinni.NodeWorker
{
    public class Program
    {
        private const int DefaultTimeoutSec = 5 * 60;


        public static int Main()
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
                var serviceTimeout = TimeSpan.FromSeconds(serviceOptions.PackageTimeout ?? DefaultTimeoutSec);

                config.SetStartTimeout(serviceTimeout);
                config.SetStopTimeout(serviceTimeout);

                config.Service<Tuple<IAppServiceHost, AppServiceHostPipeServer>>(s =>
                {
                    s.ConstructUsing(hostSettings =>
                    {
                        var serviceHost = new AppServiceHostDomainProxy(serviceOptions);
                        var serviceHostPipeServer = new AppServiceHostPipeServer(serviceOptions, serviceHost);
                        var instance = new Tuple<IAppServiceHost, AppServiceHostPipeServer>(serviceHost, serviceHostPipeServer);

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


        private static AppServiceOptions ParseServiceOptions(IDictionary<string, object> parameters)
        {
            return new AppServiceOptions
            {
                PackageId = GetParameterValue(parameters, "packageId", "Infinni.NodeWorker"),
                PackageVersion = GetParameterValue(parameters, "packageVersion", "1.0.0"),
                PackageInstance = GetParameterValue(parameters, "packageInstance"),
                PackageConfig = GetParameterValue(parameters, "packageConfig"),
                PackageDirectory = GetParameterValue(parameters, "packageDirectory"),
                PackageTimeout = GetPackageTimeout(GetParameterValue(parameters, "packageTimeout"))
            };
        }

        private static string GetParameterValue(IDictionary<string, object> parameters, string parameterName, string defaultValue = null)
        {
            object value;

            if (parameters.TryGetValue(parameterName, out value) && !string.IsNullOrWhiteSpace(value as string))
            {
                return ((string)value).Trim();
            }

            return defaultValue;
        }

        private static int? GetPackageTimeout(string value)
        {
            int packageTimeout;

            return (!string.IsNullOrWhiteSpace(value) && int.TryParse(value.Trim(), out packageTimeout) && packageTimeout >= 0)
                ? packageTimeout
                : default(int?);
        }

        private static string GetServiceName(AppServiceOptions serviceOptions)
        {
            return CommonHelpers.GetAppName(serviceOptions.PackageId, serviceOptions.PackageVersion);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Default.Fatal(e.ExceptionObject);
        }
    }
}