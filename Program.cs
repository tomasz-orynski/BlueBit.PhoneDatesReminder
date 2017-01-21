using System.Diagnostics;
using BlueBit.PhoneDatesReminder.Components;
using BlueBit.PhoneDatesReminder.Components.Cfg;

namespace BlueBit.PhoneDatesReminder
{
    public static class Program
    {
        class ParsedDataToSend : 
            DataBase<DownloadedDataToParse>,
            Sender.InputData,
            Parser.OutputData
            {
                public string Content {get;set;}
                public string Title {get;set;}
            }
        class DownloadedDataToParse : 
            DataBase<ConfiguredDataToDownload>,
            Parser.InputData,
            Downloader.OutputData
            {
                public string Content {get;set;}
                public string Title {get;set;}
            }   
            
        public class ConfiguredDataToDownload : 
            DataBase,
            Downloader.InputData
            {
            }

        public static void Main(string[] args)
        {
            Debug.Assert(args.Length == 1);
            Builder
                .Start(()=>new Configurator<ConfiguredDataToDownload>())
                .Then(()=>new Downloader<ConfiguredDataToDownload, DownloadedDataToParse>())
                .Then(()=>new Parser<DownloadedDataToParse, ParsedDataToSend>())
                .Then(()=>new Sender<ParsedDataToSend>())
                .Call(args[0]);
        }
    }
}
