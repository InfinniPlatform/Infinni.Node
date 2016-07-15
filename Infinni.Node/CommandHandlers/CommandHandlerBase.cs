using System.Threading.Tasks;

namespace Infinni.Node.CommandHandlers
{
    /// <summary>
    /// Базовый класс обработчика команд.
    /// </summary>
    /// <typeparam name="TOptions">Параметры команды.</typeparam>
    public abstract class CommandHandlerBase<TOptions> : ICommandHandler
    {
        public Task Handle(object options)
        {
            return Handle((TOptions)options);
        }

        public abstract Task Handle(TOptions options);
    }
}