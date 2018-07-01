using DefensiveProgrammingFramework;
using Serilog;
using System;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder
{
    public static class Diagnostics
    {
        public static Func<TIn, Task<TOut>> WithLogStartStop<TIn, TOut>(this Func<TIn, Task<TOut>> @this)
        {
            @this.CannotBeNull();
            return async input =>
            {
                Log.Logger.Information("### START ###");
                try
                {
                    return await @this(input);
                }
                finally
                {
                    Log.Logger.Information("### STOP ###");
                }
            };
        }

        public static async Task<TOut> CallWithLogInfoAsync<T, TOut>(this Func<T> @this, Func<T, Task<TOut>> call)
        {
            @this.CannotBeNull();
            @call.CannotBeNull();
            var tmp = @this();
            var info = tmp.GetType().Name;

            Log.Logger.Information($"=>{info}");
            try
            {
                return await call(tmp);
            }
            finally
            {
                Log.Logger.Information($"<={info}");
            }
        }
    }
}