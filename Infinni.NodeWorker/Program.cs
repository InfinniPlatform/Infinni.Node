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
            /* Вся логика метода Main() находится в отдельных методах, чтобы JIT-компиляция Main()
             * прошла без загрузки дополнительных сборок, поскольку до этого момента нужно успеть
             * установить свою собственную логику загрузки сборок.
             */

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Устанавливает для приложения контекст загрузки сборок по умолчанию
            InitializeAssemblyLoadContext();

            // Запускает хостинг приложения
            return RunServiceHost();
        }


        private static void InitializeAssemblyLoadContext()
        {
            var context = new DirectoryAssemblyLoadContext();
            DirectoryAssemblyLoadContext.InitializeDefaultContext(context);
        }


        private static int RunServiceHost()
        {
            Log.Default.Info(Environment.CommandLine);

            var result = HostFactory.Run(config =>
            {
                config.UseLog4Net();

                var parameters = config.SelectPlatform(i => i
                    .AddStringParameter("packageId")
                    .AddStringParameter("packageVersion")
                    .AddStringParameter("packageInstance")
                    .AddStringParameter("packageDirectory")
                    .AddStringParameter("packageTimeout"));

                var serviceOptions = ParseServiceOptions(parameters);
                var serviceTimeout = TimeSpan.FromSeconds(serviceOptions.PackageTimeout ?? DefaultTimeoutSec);

                config.SetStartTimeout(serviceTimeout);
                config.SetStopTimeout(serviceTimeout);

                config.Service<AppServiceHost>(s =>
                {
                    s.ConstructUsing(hostSettings =>
                    {
                        var instance = new AppServiceHost();
                        return instance;
                    });

                    s.WhenStarted((instance, hostControl) =>
                    {
                        instance.Start(serviceTimeout);
                        return true;
                    });

                    s.WhenStopped((instance, hostControl) =>
                    {
                        instance.Stop(serviceTimeout);
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
            return $"{serviceOptions.PackageId}.{serviceOptions.PackageVersion}";
        }


        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Default.Fatal(e.ExceptionObject);
        }
    }
}