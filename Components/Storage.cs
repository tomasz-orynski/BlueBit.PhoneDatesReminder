using System;
using System.Diagnostics;
using System.IO;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class Storage
    {
        public interface InputData
        {
            Cfg.StorageCfg StorageCfg {get;}
            DateTime Date {get;}
        }
    }

    public abstract class StorageBase<T> : 
        ComponentBase<T>
        where T: class, Storage.InputData
    {
        override sealed protected void OnWork(T input)
        {
            Debug.Assert(input.StorageCfg != null);
            OnWorkWithPath(Path.Combine(input.StorageCfg.Path, input.Date.ToString("yyyyMMdd")));
        }
        protected abstract void OnWorkWithPath(string path);
    }

    public class StorageCheck<T> : 
        StorageBase<T>
        where T: class, Storage.InputData
    {
        override protected void OnWorkWithPath(string path)
        {
            if (File.Exists(path))
                Break();
        }
    }
    public class StorageSave<T> : 
        StorageBase<T>
        where T: class, Storage.InputData
    {
        override protected void OnWorkWithPath(string path)
        {
            using (File.Create(path));
        }
    }
}
