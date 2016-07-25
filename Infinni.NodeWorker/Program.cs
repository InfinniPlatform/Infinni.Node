using System;
using System.Collections.Generic;
using System.IO;

using Infinni.NodeWorker.Services;

using Topshelf;

namespace Infinni.NodeWorker
{
    public class Program
    {
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
            Console.WriteLine(Environment.CommandLine);

            var result = HostFactory.Run(config =>
                                         {
                                             // Чтение параметров командной строки

                                             var parameters = config.SelectPlatform(i => i
                                                 .AddStringParameter("packageId")
                                                 .AddStringParameter("packageVersion")
                                                 .AddStringParameter("packageInstance")
                                                 .AddStringParameter("packageDirectory")
                                                 .AddStringParameter("packageTimeout"));

                                             var serviceOptions = ParseServiceOptions(parameters);

                                             // Установка текущего каталога приложения
                                             Directory.SetCurrentDirectory(serviceOptions.PackageDirectory);

                                             // Установка таймаута для запуска и остановки

                                             var serviceTimeout = TimeSpan.MaxValue;

                                             if (serviceOptions.PackageTimeout != null)
                                             {
                                                 serviceTimeout = TimeSpan.FromSeconds(serviceOptions.PackageTimeout.Value);
                                                 config.SetStartTimeout(serviceTimeout);
                                                 config.SetStopTimeout(serviceTimeout);
                                             }

                                             config.Service<AppServiceHost>(s =>
                                                                            {
                                                                                // Создание экземпляра приложения
                                                                                s.ConstructUsing(hostSettings =>
                                                                                                 {
                                                                                                     try
                                                                                                     {
                                                                                                         var instance = new AppServiceHost();
                                                                                                         return instance;
                                                                                                     }
                                                                                                     catch (Exception exception)
                                                                                                     {
                                                                                                         Console.WriteLine(exception);
                                                                                                         throw;
                                                                                                     }
                                                                                                 });

                                                                                // Запуск экземпляра приложения
                                                                                s.WhenStarted((instance, hostControl) =>
                                                                                              {
                                                                                                  try
                                                                                                  {
                                                                                                      instance.Start(serviceTimeout);
                                                                                                      return true;
                                                                                                  }
                                                                                                  catch (Exception exception)
                                                                                                  {
                                                                                                      Console.WriteLine(exception);
                                                                                                      throw;
                                                                                                  }
                                                                                              });

                                                                                // Остановка экземпляра приложения
                                                                                s.WhenStopped((instance, hostControl) =>
                                                                                              {
                                                                                                  try
                                                                                                  {
                                                                                                      instance.Stop(serviceTimeout);
                                                                                                      return true;
                                                                                                  }
                                                                                                  catch (Exception exception)
                                                                                                  {
                                                                                                      Console.WriteLine(exception);
                                                                                                      throw;
                                                                                                  }
                                                                                              });
                                                                            });

                                             // Установка имени службы

                                             var serviceName = GetServiceName(serviceOptions);
                                             config.SetServiceName(serviceName);
                                             config.SetDisplayName(serviceName);
                                             config.SetDescription(serviceName);

                                             // Установка экземпляра службы

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
            Console.WriteLine(e.ExceptionObject);
        }
    }
}