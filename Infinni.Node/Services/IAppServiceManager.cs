using System.Threading.Tasks;

using Infinni.Node.Packaging;

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
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        Task Install(InstallDirectoryItem appInstallation);

        /// <summary>
        /// Удаляет сервис приложения.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        Task Uninstall(InstallDirectoryItem appInstallation);

        /// <summary>
        /// Запускает сервис приложения.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        /// <param name="timeoutSeconds">Таймаут выполнения операции (в секундах).</param>
        Task Start(InstallDirectoryItem appInstallation, int? timeoutSeconds = null);

        /// <summary>
        /// Останавливает сервис приложения.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        /// <param name="timeoutSeconds">Таймаут выполнения операции (в секундах).</param>
        Task Stop(InstallDirectoryItem appInstallation, int? timeoutSeconds = null);

        /// <summary>
        /// Возвращает состояние сервиса приложения.
        /// </summary>
        /// <param name="appInstallation">Сведения об установке приложения.</param>
        /// <param name="timeoutSeconds">Таймаут выполнения операции (в секундах).</param>
        Task<object> GetStatus(InstallDirectoryItem appInstallation, int? timeoutSeconds = null);
    }
}