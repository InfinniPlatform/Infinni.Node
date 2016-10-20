using CommandLine;

namespace Infinni.Node.CommandOptions
{
    [Verb("packages", HelpText = "Return available packages list.")]
    public class PackagesCommandOptions
    {
        [Option(
             'i',
             "id",
             Required = true,
             HelpText = "Specifies package search term."
         )]
        public string Id { get; set; }

        [Option(
             'p',
             "allowPrerelease",
             Required = false,
             HelpText = "Indicates whether this command will consider prerelease packages. " +
                        "If omitted, only stable packages are considered."
         )]
        public bool AllowPrereleaseVersions { get; set; }

        [Option(
             'f',
             "format",
             Required = false,
             Default = false,
             HelpText = "Indicates whether this command will format output. " +
                        "If omitted, defaults none formatting."
         )]
        public bool Format { get; set; }
    }
}