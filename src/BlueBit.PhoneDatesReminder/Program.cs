using BlueBit.PhoneDatesReminder.Components;
using BlueBit.PhoneDatesReminder.Components.Cfg;
using System;
using System.Diagnostics;
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
            public DateTime Date { get; set; }
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
            Downloader.InputData
        {
        }

        public static async Task Main(string[] args)
        {
            Debug.Assert(args.Length == 1);
            var result = await Runner
                .Start(() => new Configurator<DataFromCfg>())
                .Then(() => new Downloader<DataFromCfg, DataFromWeb>())
                .Then(() => new Parser<DataFromWeb, Data>())
                .Then(() => new StorageCheck<Data>())
                .Then(() => new SenderSms<Data>())
                .Then(() => new SenderSmtp<Data>())
                .Then(() => new StorageSave<Data>())
                .RunAsync(args[0]);
        }
    }
}