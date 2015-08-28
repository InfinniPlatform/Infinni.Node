using CommandLine;

namespace Infinni.Node.CommandOptions
{
	[Verb("status", HelpText = "Shows status of worker processes.")]
	internal sealed class StatusCommandOptions
	{
		[Option(
			'i',
			"id",
			Required = false,
			HelpText = "Specifies the package ID of the package to get status. " +
					   "If omitted, shows status all worker processes."
			)]
		public string Id { get; set; }

		[Option(
			'v',
			"version",
			Required = false,
			HelpText = "Specifies the version of the package to get status. " +
					   "If omitted, defaults to all versions."
			)]
		public string Version { get; set; }

		[Option(
			'n',
			"instance",
			Required = false,
			HelpText = "Specifies the instance name of the package to get status (if the package has installed multiple times). " +
					   "If omitted, defaults to all instances."
			)]
		public string Instance { get; set; }
	}
}