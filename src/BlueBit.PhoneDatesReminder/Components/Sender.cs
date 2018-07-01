using BlueBit.PhoneDatesReminder.Commons.Extensions;
using BlueBit.PhoneDatesReminder.Components.Cfg;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class Sender
    {
        public interface InputData
        {
            IReadOnlyList<(string PhoneNumber, DateTime Date, Reason Reason)> Items { get; }
            Cfg.StorageCfg StorageCfg { get; }
        }
    }

    public abstract class Sender<T> :
        ComponentBase<T>
        where T : class, Sender.InputData
    {
        protected abstract string Name { get; }

        protected static string GetMsg((string PhoneNumber, DateTime Date, Reason Reason) item)
            => $"Dnia [{item.Date.ToString("yyyy-MM-dd")}] dla numeru [{item.PhoneNumber}] upływa termin [{item.Reason.AsDescription()}]!";

        override protected async Task OnWorkAsync(T input)
        {
            var path = input.StorageCfg.GetDirPath();
            await GetTasks(input)
                .Select(_ => Handle(_, path))
                .CallAsync();
        }

        protected abstract IEnumerable<(string Code, Func<Task> Action)> GetTasks(T input);

        private Func<Task> Handle((string Code, Func<Task> Action) item, string path)
            => async () => {
                var log = Log.ForContext<Sender<T>>();

                var fileName = $"{Name}#{item.Code}";
                log.Information("Check send '{Item}'", fileName);
                var filePath = Path.Combine(path, fileName);
                if (File.Exists(filePath))
                    return;

                await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(
                        RetrySleepDurations,
                        (ex, ts) => log.Warning(ex, "Cannot send '{Item}' after {SleepDuration}", fileName, ts)
                    )
                    .ExecuteAsync(async () => {
                        log.Information("Begin send '{Item}'", fileName);
                        await item.Action();
                        await File.WriteAllBytesAsync(filePath, new byte[] { });
                        log.Information("End send '{Item}'", fileName);
                    });
            };
    }
}
