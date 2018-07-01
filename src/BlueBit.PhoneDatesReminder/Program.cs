using BlueBit.PhoneDatesReminder.Components;
using BlueBit.PhoneDatesReminder.Components.Cfg;
using DefensiveProgrammingFramework;
using Serilog;
using System;
using System.Collections.Generic;
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

        static Program()
        {
            var template = "{Timestamp:yyyyMMdd#HHmmss.fffzzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Trace(outputTemplate: template)
                .WriteTo.ColoredConsole(outputTemplate: template)
                .WriteTo.File("..\\logs\\PhoneDatesReminder.DailyLog-.txt", rollingInterval: RollingInterval.Day, outputTemplate: template)
                .CreateLogger();
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
                .ThenMany(
                    () => new SenderSms<Data>(),
                    () => new SenderSmtp<Data>())
                .RunAsync(args[0]);
        }
    }
}
