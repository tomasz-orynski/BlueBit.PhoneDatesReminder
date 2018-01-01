using BlueBit.PhoneDatesReminder.Commons.Extensions;
using System;
using System.Collections.Generic;

namespace BlueBit.PhoneDatesReminder.Components
{
    public static class Sender
    {
        public interface InputData
        {
            IReadOnlyList<(string PhoneNumber, DateTime Date, Reason Reason)> Items { get; }
        }
    }

    public abstract class Sender<T> :
        ComponentBase<T>
        where T : class, Sender.InputData
    {
        protected static string GetMsg((string PhoneNumber, DateTime Date, Reason Reason) item)
            => $"Dnia [{item.Date.ToString("yyyy-MM-dd")}] dla numeru [{item.PhoneNumber}] upływa termin [{item.Reason.AsDescription()}]!";
    }
}
