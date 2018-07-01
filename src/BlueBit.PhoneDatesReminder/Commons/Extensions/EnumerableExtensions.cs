using DefensiveProgrammingFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Commons.Extensions
{
    public static class EnumerableExtensions
    {
        private static Random _rnd = new Random();

        public static async Task CallAsync(this IEnumerable<Func<Task>> @this)
        {
            @this.CannotBeNull();
            await Task.WhenAll(@this.Select(@_ => @_.Invoke()));
        }

        public static IEnumerable<T> AsRandomAndCyclic<T>(this IEnumerable<T> @this)
        {
            @this.CannotBeNull();
            while(true)
            {
                foreach (var item in @this.OrderBy(_ => _rnd.Next()))
                    yield return item;
            }
        }
    }
}
