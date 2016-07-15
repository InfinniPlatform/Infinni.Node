using CommandLine;

namespace Infinni.Node.CommandOptions
{
    [Verb("stop", HelpText = "Stops a worker process carefully.")]
    public class StopCommandOptions
    {
        [Option(
            'i',
            "id",
            Required = false,
            HelpText = "Specifies the package ID of the package to stop. " +
                       "If omitted, stops carefully all started packages."
            )]
        public string Id { get; set; }

        [Option(
            'v',
            "version",
            Required = false,
            HelpText = "Specifies the version of the package to stop. " +
                       "If omitted, defaults to all versions."
            )]
        public string Version { get; set; }

        [Option(
            'n',
            "instance",
            Required = false,
            HelpText = "Specifies the instance name of the package to stop (if the package has installed multiple times). " +
                       "If omitted, defaults to all instances."
            )]
        public string Instance { get; set; }

        [Option(
            't',
            "timeout",
            Required = false,
            HelpText = "Specifies timeout (is seconds) of the package to stop. " +
                       "If omitted, defaults infinite."
            )]
        public int? Timeout { get; set; }
    }
}