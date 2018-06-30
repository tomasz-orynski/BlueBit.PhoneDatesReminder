using DefensiveProgrammingFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class Parser
    {
        public interface InputData
        {
            Cfg.ParserCfg ParserCfg { get; }
            string Content { get; }
        }

        public interface OutputData
        {
            IReadOnlyList<(string PhoneNumber, DateTime Date, Reason Reason)> Items { set; }
        }
    }

    public class Parser<TIn, TOut> :
        ComponentBase<TIn, TOut>
        where TIn : class, Parser.InputData
        where TOut : Parser.OutputData, new()
    {
        private const int ValueCountInRow = 7;
        private const int ValueNumberIndex = 1;
        private static readonly IReadOnlyDictionary<Reason, int> Reason2ValueIndex = new Dictionary<Reason, int>()
        {
            [Reason.Internet] = 4,
            [Reason.Payment] = 6,
        };

        override protected async Task OnWorkAsync(TIn input, TOut output)
        {
            input.ParserCfg.CannotBeNull();
            input.Content.CannotBeEmpty();

            var dt = Now + TimeSpan.FromDays(input.ParserCfg.DaysCnt);
            IEnumerable<(string PhoneNumber, DateTime Date, Reason Reason)> ParseLine(string[] fields)
            {
                foreach(var item in Reason2ValueIndex)
                {
                    var fieldDate = DateTime.Parse(fields[item.Value]);
                    if (fieldDate > dt) continue;
                    yield return (fields[ValueNumberIndex], fieldDate, item.Key);
                }
            }

            var items = input.Content
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(_ => _.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Where(_ => _?.Length == ValueCountInRow)
                .SelectMany(ParseLine)
                .ToList();

            if (items.Count == 0)
                Break();

            output.Items = items;
        }
    }
}