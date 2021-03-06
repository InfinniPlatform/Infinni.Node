﻿using CommandLine;

namespace Infinni.Node.CommandOptions
{
    [Verb("start", HelpText = "Starts a worker process.")]
    public class StartCommandOptions
    {
        [Option(
            'i',
            "id",
            Required = false,
            HelpText = "Specifies the package ID of the package to start. " +
                       "If omitted, starts all installed packages."
            )]
        public string Id { get; set; }

        [Option(
            'v',
            "version",
            Required = false,
            HelpText = "Specifies the version of the package to start. " +
                       "If omitted, defaults to all versions."
            )]
        public string Version { get; set; }

        [Option(
            'n',
            "instance",
            Required = false,
            HelpText = "Specifies the instance name of the package to start (if the package has installed multiple times). " +
                       "If omitted, defaults to all instances."
            )]
        public string Instance { get; set; }

        [Option(
            't',
            "timeout",
            Required = false,
            HelpText = "Specifies timeout (is seconds) of the package to start. " +
                       "If omitted, defaults infinite."
            )]
        public int? Timeout { get; set; }
    }
}