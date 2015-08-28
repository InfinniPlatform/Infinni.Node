using System.Threading.Tasks;

namespace Infinni.Node.Worker
{
	/// <summary>
	/// Менеджер по работе с сервисами рабочих процессов.
	/// </summary>
	internal interface IWorkerServiceManager
	{
		/// <summary>
		/// Устанавливает сервис рабочего процесса.
		/// </summary>
		/// <param name="options">Параметры сервиса рабочего процесса.</param>
		Task Install(WorkerServiceOptions options);

		/// <summary>
		/// Удаляет сервис рабочего процесса.
		/// </summary>
		/// <param name="options">Параметры сервиса рабочего процесса.</param>
		Task Uninstall(WorkerServiceOptions options);

		/// <summary>
		/// Запускает сервис рабочего процесса.
		/// </summary>
		/// <param name="options">Параметры сервиса рабочего процесса.</param>
		Task Start(WorkerServiceOptions options);

		/// <summary>
		/// Останавливает сервис рабочего процесса.
		/// </summary>
		/// <param name="options">Параметры сервиса рабочего процесса.</param>
		Task Stop(WorkerServiceOptions options);

		/// <summary>
		/// Возвращает состояние сервиса рабочего процесса.
		/// </summary>
		/// <param name="options">Параметры сервиса рабочего процесса.</param>
		Task<object> GetStatus(WorkerServiceOptions options);
	}
}