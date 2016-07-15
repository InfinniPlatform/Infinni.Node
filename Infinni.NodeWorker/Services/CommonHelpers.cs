namespace Infinni.NodeWorker.Services
{
    public static class CommonHelpers
    {
        public const char InstanceDelimiter = '@';

        public static string GetAppName(string packageId, string packageVersion, string instance = null)
        {
            var packageName = string.IsNullOrWhiteSpace(packageVersion) ? packageId : $"{packageId}.{packageVersion}";

            return string.IsNullOrWhiteSpace(instance) ? packageName : $"{packageName}{InstanceDelimiter}{instance}";
        }
    }
}