using System;
using System.Linq;

using Infinni.Node.Packaging;

namespace Infinni.Node.CommandHandlers
{
	internal static class CommandHandlerHelpers
	{
		public static InstallDirectoryItem[] GetItems(this IInstallDirectoryManager installDirectory, string packageId, string packageVersion, string packageInstance)
		{
			var installItems = installDirectory.GetItems();

			if (!string.IsNullOrWhiteSpace(packageId))
			{
				installItems = installItems.Where(i => string.Equals(i.PackageName.Id, packageId.Trim(), StringComparison.OrdinalIgnoreCase));
			}

			if (!string.IsNullOrWhiteSpace(packageVersion))
			{
				installItems = installItems.Where(i => string.Equals(i.PackageName.Version, packageVersion.Trim(), StringComparison.OrdinalIgnoreCase));
			}

			if (!string.IsNullOrWhiteSpace(packageInstance))
			{
				installItems = installItems.Where(i => string.Equals(i.Instance, packageInstance.Trim(), StringComparison.OrdinalIgnoreCase));
			}

			return installItems.ToArray();
		}
	}
}