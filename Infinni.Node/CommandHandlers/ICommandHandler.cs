using System.Threading.Tasks;

namespace Infinni.Node.CommandHandlers
{
    /// <summary>
    /// Интерфейс обработчика команд.
    /// </summary>
    public interface ICommandHandler
    {
        /// <summary>
        /// Обрабатывает команду.
        /// </summary>
        /// <param name="options">Параметры команды.</param>
        Task Handle(object options);
    }
}