using System;
using System.Collections.Generic;

using log4net;

namespace Infinni.Node.CommandHandlers
{
    public class CommandRunner
    {
        public CommandRunner(ILog log)
        {
            _log = log;
            _commands = new Dictionary<Type, Func<ICommandHandler>>();
        }


        private readonly ILog _log;
        private readonly Dictionary<Type, Func<ICommandHandler>> _commands;


        public IEnumerable<Type> GetOptionsTypes()
        {
            return _commands.Keys;
        }

        public CommandRunner RegisterCommand<TOptions>(Func<CommandHandlerBase<TOptions>> handler)
        {
            _commands[typeof(TOptions)] = handler;

            return this;
        }

        public bool HandleCommand(object options)
        {
            if (options != null)
            {
                Func<ICommandHandler> handlerFactory;

                if (_commands.TryGetValue(options.GetType(), out handlerFactory))
                {
                    try
                    {
                        _log.Info(Environment.CommandLine);

                        AsyncHelper.RunSync(() => handlerFactory().Handle(options));

                        return true;
                    }
                    catch (Exception error)
                    {
                        _log.Error(error);
                    }
                }
            }

            return false;
        }
    }
}