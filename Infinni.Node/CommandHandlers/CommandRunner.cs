using System;
using System.Collections.Generic;

using Infinni.Node.Logging;

namespace Infinni.Node.CommandHandlers
{
	internal sealed class CommandRunner
	{
		private readonly Dictionary<Type, Action<CommandContext, object>> _commands
			= new Dictionary<Type, Action<CommandContext, object>>();

		public IEnumerable<Type> GetOptionsTypes()
		{
			return _commands.Keys;
		}

		public CommandRunner RegisterCommand<TOptions, THandler>() where THandler : new()
		{
			_commands[typeof(TOptions)] = (context, options) =>
			{
				dynamic handler = new THandler();
				handler.Handle(context, (TOptions)options);
			};

			return this;
		}

		public bool HandleCommand(CommandContext context, object options)
		{
			if (options != null)
			{
				Action<CommandContext, object> handler;

				if (_commands.TryGetValue(options.GetType(), out handler))
				{
					try
					{
						Log.Default.Info(Environment.CommandLine);

						handler(context, options);
						return true;
					}
					catch (Exception error)
					{
						Log.Default.Error(error);
					}
				}
			}

			return false;
		}
	}
}