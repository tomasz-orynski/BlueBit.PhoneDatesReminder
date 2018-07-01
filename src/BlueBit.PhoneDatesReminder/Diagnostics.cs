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
    }
}