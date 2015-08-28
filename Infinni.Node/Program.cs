using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;

using Infinni.Node.CommandHandlers;
using Infinni.Node.CommandOptions;
using Infinni.Node.Logging;

namespace Infinni.Node
{
	class Program
	{
		private static readonly CommandRunner CommandRunner = new CommandRunner()
			.RegisterCommand<InstallCommandOptions, InstallCommandHandler>()
			.RegisterCommand<UninstallCommandOptions, UninstallCommandHandler>()
			.RegisterCommand<StartCommandOptions, StartCommandHandler>()
			.RegisterCommand<StopCommandOptions, StopCommandHandler>()
			.RegisterCommand<StatusCommandOptions, StatusCommandHandler>()
			;


		private static int Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

			var options = ParseCommandOptions(args, CommandRunner.GetOptionsTypes());

			if (options != null)
			{
				var context = new CommandContext();

				if (CommandRunner.HandleCommand(context, options))
				{
					return 0;
				}
			}

			return 1;
		}

		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Default.Fatal(e.ExceptionObject);
		}

		private static object ParseCommandOptions(IEnumerable<string> args, IEnumerable<Type> optionsTypes)
		{
			object options = null;

			var optionsParser = Parser.Default;
			var optionsTypesArray = optionsTypes.ToArray();

			try
			{
				var result = optionsParser.ParseArguments(args, optionsTypesArray) as Parsed<object>;

				if (result != null)
				{
					options = result.Value;
				}
			}
			catch
			{
				optionsParser.ParseArguments(new[] { "help" }, optionsTypesArray);
			}

			return options;
		}
	}
}