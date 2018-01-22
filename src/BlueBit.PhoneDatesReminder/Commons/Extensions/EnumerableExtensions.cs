using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Commons.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task CallAsync(this IEnumerable<Func<Task>> @this)
        {
            Contract.Assert(@this != null);
            foreach (var task in @this)
                await task();
        }
    }
}
