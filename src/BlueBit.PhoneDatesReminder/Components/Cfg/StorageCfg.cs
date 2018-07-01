using DefensiveProgrammingFramework;
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
            @this.CannotBeNull();
            @this.Path.CannotBeEmpty();
            const string frmt = "yyyyMMdd";
            return Path.Combine(@this.Path, ComponentBase.Now.ToString(frmt));
        }
    }
}