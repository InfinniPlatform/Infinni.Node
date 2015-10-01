using System;
using System.Linq;
using System.Security.Principal;

using Infinni.Node.Packaging;
using Infinni.Node.Properties;

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

		public static void CheckAdministrativePrivileges()
		{
			if (!IsRunningAsRoot())
			{
				throw new InvalidOperationException(Resources.YouMustHaveAdministrativePrivilegesToRunThisCommand);
			}
		}

		private static bool IsRunningAsRoot()
		{
			if (MonoHelper.RunningOnMono)
			{
				return MonoHelper.RunningAsRoot;
			}

			try
			{
				// Этот код также работает под Linux, но есть сомнения, что под любой платформой

				var user = WindowsIdentity.GetCurrent();

				if (user != null)
				{
					var principal = new WindowsPrincipal(user);

					return principal.IsInRole(WindowsBuiltInRole.Administrator);
				}
			}
			catch (Exception)
			{
			}

			return false;
		}
	}
}