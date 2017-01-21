using System;
using System.Diagnostics;

namespace BlueBit.PhoneDatesReminder.Components.Cfg
{
    public class ParserCfg
    {
        public int DaysCnt {get; set;}
    }
    public class SenderCfg
    {
        public string Email {get; set;}
        public string Host {get;set;}
        public int Port {get;set;}
        public string User {get;set;}
        public string Password {get;set;}
    }

    public class DownloaderCfg
    {
        public string Url {get;set;}
    }

    public interface IInitialize<in TBase>
    {
        void Init(TBase data);
    }

    public class DataBase
    {
        public DownloaderCfg DownloaderCfg {get;set;}
        public ParserCfg ParserCfg {get;set;}
        public SenderCfg SenderCfg {get;set;}
    }

    class DataBase<T> :
        DataBase,
        IInitialize<T>
        where T: DataBase
    {
        public void Init(T prev)
        {
            DownloaderCfg = prev.DownloaderCfg;
            ParserCfg = prev.ParserCfg;
            SenderCfg = prev.SenderCfg;
        }
    }
}