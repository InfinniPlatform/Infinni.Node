namespace Infinni.NodeWorker.Services
{
    /// <summary>
    /// Параметры сервиса приложения.
    /// </summary>
    public class AppServiceOptions
    {
        /// <summary>
        /// ID пакета приложения.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// Версия пакета приложения.
        /// </summary>
        public string PackageVersion { get; set; }

        /// <summary>
        /// Экземпляр версии приложения.
        /// </summary>
        public string PackageInstance { get; set; }

        /// <summary>
        /// Рабочий каталог приложения.
        /// </summary>
        public string PackageDirectory { get; set; }

        /// <summary>
        /// Таймаут выполнения операции.
        /// </summary>
        public int? PackageTimeout { get; set; }

        /// <summary>
        /// Дополнительные параметры запуска приложения.
        /// </summary>
        public string StartOptions { get; set; }
    }
}