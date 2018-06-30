using System.Collections.Generic;

namespace BlueBit.PhoneDatesReminder.Components.Cfg
{
    public class SenderSmsCfg
    {
        public IReadOnlyList<string> Urls { get; set; }
        public IReadOnlyCollection<string> Phones { get; set; }
    }
}