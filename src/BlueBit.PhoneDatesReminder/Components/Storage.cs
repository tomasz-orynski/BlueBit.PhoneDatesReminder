using BlueBit.PhoneDatesReminder.Components.Cfg;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class Storage
    {
        public interface InputData
        {
            Cfg.StorageCfg StorageCfg { get; }
        }
    }

    public abstract class StorageBase<T> :
        ComponentBase<T>
        where T : class, Storage.InputData
    {
        override sealed protected Task OnWorkAsync(T input)
        {
            Debug.Assert(input.StorageCfg != null);

            var path = input.StorageCfg.GetDirPath();
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return OnWorkWithPathAsync(path);
        }

        protected abstract Task OnWorkWithPathAsync(string path);
    }

    public class StorageCheck<T> :
        StorageBase<T>
        where T : class, Storage.InputData
    {
        override protected async Task OnWorkWithPathAsync(string path)
        {
        }
    }
}