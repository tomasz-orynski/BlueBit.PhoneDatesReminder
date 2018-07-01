using DefensiveProgrammingFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Commons.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task CallAsync(this IEnumerable<Func<Task>> @this)
        {
            @this.CannotBeNull();
            await Task.WhenAll(@this.Select(@_ => @_.Invoke()));
        }
    }
}
