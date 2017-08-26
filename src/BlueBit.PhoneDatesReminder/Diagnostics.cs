using System;
using System.Diagnostics;

namespace BlueBit.PhoneDatesReminder
{
    public static class Diagnostics
    {
        public static Func<TIn, TOut> WithLogStartStop<TIn, TOut>(this Func<TIn, TOut> @this)
        {
            Debug.Assert(@this != null);
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

        public static TOut CallWithLogInfo<T, TOut>(this Func<T> @this, Func<T, TOut> call)
        {
            Debug.Assert(@this != null);
            Debug.Assert(@call != null);
            var tmp = @this();
            var info = tmp.GetType().Name;
            Console.WriteLine($"{DateTime.Now}=>{info}");
            try
            {
                return call(tmp);
            }
            finally
            {
                Console.WriteLine($"{DateTime.Now}<={info}");
            }
        }
    }
}