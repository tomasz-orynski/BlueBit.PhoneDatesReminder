using DefensiveProgrammingFramework;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder
{
    public static class Diagnostics
    {
        public static Func<TIn, TOut> WithLogStartStop<TIn, TOut>(this Func<TIn, TOut> @this)
        {
            @this.CannotBeNull();
            return input =>
            {
                Console.WriteLine($"{DateTime.Now}=>START");
                try
                {
                    return @this(input);
                }
                finally
                {
                    Console.WriteLine($"{DateTime.Now}<=STOP");
                }
            };
        }

        public static async Task<TOut> CallWithLogInfoAsync<T, TOut>(this Func<T> @this, Func<T, Task<TOut>> call)
        {
            @this.CannotBeNull();
            @call.CannotBeNull();
            var tmp = @this();
            var info = tmp.GetType().Name;
            Console.WriteLine($"{DateTime.Now}=>{info}");
            try
            {
                return await call(tmp);
            }
            finally
            {
                Console.WriteLine($"{DateTime.Now}<={info}");
            }
        }
    }
}