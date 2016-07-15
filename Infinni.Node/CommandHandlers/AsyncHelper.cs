using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Infinni.Node.CommandHandlers
{
    public static class AsyncHelper
    {
        public static readonly Task EmptyTask = Task.FromResult<object>(null);

        private static readonly TaskFactory InternalTaskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        public static void RunSync(Func<Task> func)
        {
            var culture = CultureInfo.CurrentCulture;
            var cultureUi = CultureInfo.CurrentUICulture;

            var syncTask = InternalTaskFactory.StartNew(() =>
            {
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = cultureUi;
                return func();
            });

            syncTask.Unwrap().GetAwaiter().GetResult();
        }
    }
}