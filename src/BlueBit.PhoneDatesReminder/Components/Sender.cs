using BlueBit.PhoneDatesReminder.Commons.Extensions;
using BlueBit.PhoneDatesReminder.Components.Cfg;
using DefensiveProgrammingFramework;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            path.MustBeAbsoluteDirectoryPath();

            await GetTasks(input)
                .Select(i => Handle(i, path))
                .CallAsync();
        }

        protected abstract IEnumerable<(IEnumerable<string> To, string Content, Func<Task> Action)> GetTasks(T input);

        private Func<Task> Handle((IEnumerable<string> To, string Content, Func<Task> Action) item, string path)
        {
            item.To.CannotBeNull();
            item.Content.CannotBeEmpty();
            item.Action.CannotBeNull();

            return async () => {
                var log = Log.ForContext<Sender<T>>();

                var fileName = $"{Name}#({string.Join(",", item.To)})";
                log.Information("Check send '{Item}'", fileName);
                var filePath = Path.Combine(path, fileName);
                if (File.Exists(filePath))
                {
                    var content = await File.ReadAllTextAsync(filePath);
                    if (content == item.Content)
                        return;
                }

                await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(
                        RetrySleepDurations,
                        (ex, ts) => log.Warning(ex.Demystify(), "Cannot send '{Item}' after {SleepDuration}", fileName, ts)
                    )
                    .ExecuteAsync(async () => {
                        log.Information("Begin send '{Item}'", fileName);
                        await item.Action();
                        await File.WriteAllTextAsync(filePath, item.Content);
                        log.Information("End send '{Item}'", fileName);
                    });
               };
        }
    }
}
