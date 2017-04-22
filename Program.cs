using System;
using System.Diagnostics;
using BlueBit.PhoneDatesReminder.Components;
using BlueBit.PhoneDatesReminder.Components.Cfg;

namespace BlueBit.PhoneDatesReminder
{
    public static class Program
    {
        class Data : 
            DataBase<DataFromWeb>,
            Parser.OutputData,
            Sender.InputData,
            Storage.InputData
            {
                public DateTime Date {get;set;}
            }

        class DataFromWeb : 
            DataBase<DataFromCfg>,
            Downloader.OutputData,
            Parser.InputData
            {
                public string Content {get;set;}
            }
            
        class DataFromCfg : 
            DataBase,
            Downloader.InputData
            {
            }

        public static void Main(string[] args)
        {
            Debug.Assert(args.Length == 1);
            var result = Runner
                .Start(()=>new Configurator<DataFromCfg>())
                .Then(()=>new Downloader<DataFromCfg, DataFromWeb>())
                .Then(()=>new Parser<DataFromWeb, Data>())
                .Then(()=>new StorageCheck<Data>())
                .Then(()=>new Sender<Data>())
                .Then(()=>new StorageSave<Data>())
                .Run(args[0]);
        }
    }
}
