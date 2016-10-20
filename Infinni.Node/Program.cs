using System;
using System.Collections.Generic;
using System.Linq;

using CommandLine;

using Infinni.Node.CommandHandlers;
using Infinni.Node.Logging;
using Infinni.Node.Packaging;
using Infinni.Node.Services;

namespace Infinni.Node
{
    class Program
    {
        private static readonly CommandRunner CommandRunner = new CommandRunner(Log.Default)
            .RegisterCommand(() => new InstallCommandHandler(new NuGetPackageRepositoryManagerFactory(new NuGetLogger(Log.Default)), new InstallDirectoryManager(Log.Default), new AppServiceManager(), Log.Default))
            .RegisterCommand(() => new UninstallCommandHandler(new InstallDirectoryManager(Log.Default), new AppServiceManager(), Log.Default))
            .RegisterCommand(() => new InitCommandHandler(new InstallDirectoryManager(Log.Default), new AppServiceManager(), Log.Default))
            .RegisterCommand(() => new StartCommandHandler(new InstallDirectoryManager(Log.Default), new AppServiceManager(), Log.Default))
            .RegisterCommand(() => new StopCommandHandler(new InstallDirectoryManager(Log.Default), new AppServiceManager(), Log.Default))
            .RegisterCommand(() => new RestartCommandHandler(new InstallDirectoryManager(Log.Default), new AppServiceManager(), Log.Default))
            .RegisterCommand(() => new StatusCommandHandler(new InstallDirectoryManager(Log.Default), new AppServiceManager(), Log.Default))
            .RegisterCommand(() => new PackagesCommandHandler(new NuGetPackageRepositoryManagerFactory(new NuGetLogger(Log.Default)), Log.Default));


        private static int Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            var options = ParseCommandOptions(args, CommandRunner.GetOptionsTypes());

            if (options != null)
            {
                if (CommandRunner.HandleCommand(options))
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