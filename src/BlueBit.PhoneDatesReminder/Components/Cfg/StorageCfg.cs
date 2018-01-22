using System.Diagnostics;
using System.IO;

namespace BlueBit.PhoneDatesReminder.Components.Cfg
{
    public class StorageCfg
    {
        public string Path { get; set; }
    }

    public static class StorageCfgPExtensions
    {
        public static string GetDirPath(this StorageCfg @this)
        {
            Debug.Assert(@this != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(@this.Path));
            const string frmt = "yyyyMMdd";
            return Path.Combine(@this.Path, ComponentBase.Now.ToString(frmt));
        }
    }
}