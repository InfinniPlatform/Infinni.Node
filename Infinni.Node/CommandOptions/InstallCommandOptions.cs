using CommandLine;

namespace Infinni.Node.CommandOptions
{
    [Verb("install", HelpText = "Installs a package.")]
    public class InstallCommandOptions
    {
        [Option(
            'i',
            "id",
            Required = true,
            HelpText = "Specifies the package ID of the package to install."
            )]
        public string Id { get; set; }

        [Option(
            'v',
            "version",
            Required = false,
            HelpText = "Specifies the version of the package to install. " +
                       "If omitted, defaults to the latest version."
            )]
        public string Version { get; set; }

        [Option(
            'n',
            "instance",
            Required = false,
            HelpText = "Specifies the instance name of the package to install (if installing the package multiple times). " +
                       "If omitted, defaults to empty string."
            )]
        public string Instance { get; set; }

        [Option(
            's',
            "source",
            Required = false,
            HelpText = "Specifies the URL or directory path for the package source containing the package to install. " +
                       "If omitted, looks in the currently selected package source to find the corresponding package URL."
            )]
        public string Source { get; set; }

        [Option(
            'p',
            "allowPrerelease",
            Required = false,
            HelpText = "Indicates whether this command will consider prerelease packages. " +
                       "If omitted, only stable packages are considered."
            )]
        public bool AllowPrereleaseVersions { get; set; }
    }
}