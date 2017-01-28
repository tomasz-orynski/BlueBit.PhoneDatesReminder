using System;
using System.Diagnostics;

namespace BlueBit.PhoneDatesReminder.Components.Cfg
{
    public interface IInitialize<in TBase>
    {
        void Init(TBase data);
    }

    public class DataBase
    {
        public DownloaderCfg DownloaderCfg {get;set;}
        public ParserCfg ParserCfg {get;set;}
        public SenderCfg SenderCfg {get;set;}
        public StorageCfg StorageCfg {get;set;}
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
            StorageCfg = prev.StorageCfg;
        }
    }
}