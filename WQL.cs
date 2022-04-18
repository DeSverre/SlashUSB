using System.Management;

namespace SlashUSB
{
    internal static class WQL
    {

        public static ManagementObjectCollection QueryMi(string query)
        {
            ManagementObjectCollection? result = null;
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query))
            {
                result = managementObjectSearcher.Get();
            }
            return result;
        }
        public static ManagementObjectCollection QueryMi(string scope, string query)
        {
            ManagementObjectCollection? result = null;
            using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(scope, query))
            {
                result = managementObjectSearcher.Get();
            }
            return result;
        }
    }
}