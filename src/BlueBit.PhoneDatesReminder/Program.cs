using BlueBit.PhoneDatesReminder.Components;
using BlueBit.PhoneDatesReminder.Components.Cfg;
using DefensiveProgrammingFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder
{
    public static class Program
    {
        private class Data :
            DataBase<DataFromWeb>,
            Parser.OutputData,
            SenderSms.InputData,
            SenderSmtp.InputData,
            Storage.InputData
        {
            public IReadOnlyList<(string PhoneNumber, DateTime Date, Reason Reason)> Items { get; set; }
        }

        private class DataFromWeb :
            DataBase<DataFromCfg>,
            Downloader.OutputData,
            Parser.InputData
        {
            public string Content { get; set; }
        }

        private class DataFromCfg :
            DataBase,
            Downloader.InputData,
            Storage.InputData
        {
        }

        public static async Task Main(string[] args)
        {
#if DEBUG
            args.CannotBeNull();
            args.CannotBeEmpty();
            if (args.Length > 1)
                ComponentBase.SetNow(DateTime.Parse(args[1]));
#endif
            var result = await Runner
                .Start(() => new Configurator<DataFromCfg>())
                .Then(() => new StorageCheck<DataFromCfg>())
                .Then(() => new Downloader<DataFromCfg, DataFromWeb>())
                .Then(() => new Parser<DataFromWeb, Data>())
                .Then(() => new SenderSms<Data>())
                .Then(() => new SenderSmtp<Data>())
                .RunAsync(args[0]);
        }
    }
}
