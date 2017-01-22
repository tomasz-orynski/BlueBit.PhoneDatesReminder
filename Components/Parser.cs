using System;
using System.Diagnostics;
using System.Linq;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class Parser
    {
        public interface InputData
        {
            Cfg.ParserCfg ParserCfg {get;}
            string Content {get;}
        }
        public interface OutputData
        {
            string Content {set;}
            string Title {set;}
        }
    }

    public class Parser<TIn, TOut> : 
        ComponentBase<TIn, TOut>
        where TIn: class, Parser.InputData 
        where TOut: Parser.OutputData, new()
    {
        override protected void OnWork(TIn input, TOut output)
        {
            Debug.Assert(input.ParserCfg != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(input.Content));
            var minDate = input.Content
                .Split(new [] { System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                .Last()
                .Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(Convert.ToDateTime)
                .Min();

            var diff = minDate - DateTime.Now;
            if (diff.TotalDays > input.ParserCfg.DaysCnt)
                Break();

            var dt = $"{minDate.ToString("yyyy-MM-dd")}";
            output.Content = $"Dnia [{dt}] upływa termin aktywacji/zapłaty za telefon!";
            output.Title = $"Przypomnienie o terminie - {dt}";
        }
    }
}