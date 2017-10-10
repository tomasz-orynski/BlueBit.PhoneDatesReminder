namespace BlueBit.PhoneDatesReminder.Components.Cfg
{
    public interface IInitialize<in TBase>
    {
        void Init(TBase data);
    }

    public class DataBase
    {
        public DownloaderCfg DownloaderCfg { get; set; }
        public ParserCfg ParserCfg { get; set; }
        public SenderSmtpCfg SenderSmtpCfg { get; set; }
        public SenderSmsCfg SenderSmsCfg { get; set; }
        public StorageCfg StorageCfg { get; set; }
    }

    internal class DataBase<T> :
        DataBase,
        IInitialize<T>
        where T : DataBase
    {
        public void Init(T prev)
        {
            DownloaderCfg = prev.DownloaderCfg;
            ParserCfg = prev.ParserCfg;
            SenderSmsCfg = prev.SenderSmsCfg;
            SenderSmtpCfg = prev.SenderSmtpCfg;
            StorageCfg = prev.StorageCfg;
        }
    }
}