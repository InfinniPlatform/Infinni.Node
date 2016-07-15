using System.Threading.Tasks;

using Infinni.NodeWorker.Services;

namespace Infinni.Node.Services
{
    /// <summary>
    /// Менеджер по работе с сервисами приложений.
    /// </summary>
    public interface IAppServiceManager
    {
        /// <summary>
        /// Устанавливает сервис приложения.
        /// </summary>
        /// <param name="options">Параметры сервиса приложения.</param>
        Task Install(AppServiceOptions options);

        /// <summary>
        /// Удаляет сервис приложения.
        /// </summary>
        /// <param name="options">Параметры сервиса приложения.</param>
        Task Uninstall(AppServiceOptions options);

        /// <summary>
        /// Запускает сервис приложения.
        /// </summary>
        /// <param name="options">Параметры сервиса приложения.</param>
        Task Start(AppServiceOptions options);

        /// <summary>
        /// Останавливает сервис приложения.
        /// </summary>
        /// <param name="options">Параметры сервиса приложения.</param>
        Task Stop(AppServiceOptions options);

        /// <summary>
        /// Возвращает состояние сервиса приложения.
        /// </summary>
        /// <param name="options">Параметры сервиса приложения.</param>
        Task<object> GetStatus(AppServiceOptions options);
    }
}