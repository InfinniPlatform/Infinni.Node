namespace Infinni.NodeWorker.ServiceHost
{
	/// <summary>
	/// Параметры сервиса рабочего процесса.
	/// </summary>
	internal sealed class WorkerServiceHostOptions
	{
		/// <summary>
		/// ID пакета.
		/// </summary>
		public string PackageId { get; set; }

		/// <summary>
		/// Версия пакета.
		/// </summary>
		public string PackageVersion { get; set; }

		/// <summary>
		/// Экземпляр пакета.
		/// </summary>
		public string PackageInstance { get; set; }

		/// <summary>
		/// Файл конфигурации пакета.
		/// </summary>
		public string PackageConfig { get; set; }

		/// <summary>
		/// Рабочий каталог пакета.
		/// </summary>
		public string PackageDirectory { get; set; }

		/// <summary>
		/// Таймаут выполнения операции.
		/// </summary>
		public string PackageTimeout { get; set; }
	}
}