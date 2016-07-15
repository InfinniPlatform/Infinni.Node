using CommandLine;

namespace Infinni.Node.CommandOptions
{
    [Verb("uninstall", HelpText = "Uninstalls a package.")]
    public class UninstallCommandOptions
    {
        [Option(
            'i',
            "id",
            Required = false,
            HelpText = "Specifies the package ID of the package to uninstall. " +
                       "If omitted, uninstalls all installed packages."
            )]
        public string Id { get; set; }

        [Option(
            'v',
            "version",
            Required = false,
            HelpText = "The version of the package to uninstall. " +
                       "If omitted, defaults to all versions."
            )]
        public string Version { get; set; }

        [Option(
            'n',
            "instance",
            Required = false,
            HelpText = "Specifies the instance name of the package to uninstall (if the package has installed multiple times). " +
                       "If omitted, defaults to all instances."
            )]
        public string Instance { get; set; }
    }
}