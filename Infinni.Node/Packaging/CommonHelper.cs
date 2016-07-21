using System;
using System.Linq;
using System.Security.Principal;

using Infinni.Node.CommandHandlers;
using Infinni.Node.Properties;

namespace Infinni.Node.Packaging
{
    public static class CommonHelper
    {
        public const char InstanceDelimiter = '@';


        public static InstallDirectoryItem[] GetItems(this IInstallDirectoryManager installDirectory, string packageId, string packageVersion, string packageInstance)
        {
            var installItems = installDirectory.GetItems();

            if (!string.IsNullOrWhiteSpace(packageId))
            {
                installItems = installItems.Where(i => string.Equals(i.PackageId, packageId.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(packageVersion))
            {
                installItems = installItems.Where(i => string.Equals(i.PackageVersion, packageVersion.Trim(), StringComparison.OrdinalIgnoreCase));
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
                throw new CommandHandlerException(Resources.YouMustHaveAdministrativePrivilegesToRunThisCommand);
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
                // Этот код также работает под Mono/Linux, но есть сомнения, что под любой платформой

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


        public static string GetAppName(string packageId, string packageVersion = null, string instance = null)
        {
            var packageName = string.IsNullOrWhiteSpace(packageVersion) ? packageId : $"{packageId}.{packageVersion}";

            return string.IsNullOrWhiteSpace(instance) ? packageName : $"{packageName}{InstanceDelimiter}{instance}";
        }
    }
}