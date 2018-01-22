using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BlueBit.PhoneDatesReminder.Components
{
    public sealed class BreakException : Exception { }

    public sealed class Void { }

    public enum Reason
    {
        [Description("Aktywacja DIL")]
        Internet = 1,
        [Description("Do³adowanie 100")]
        Payment,
    }

    public interface IComponent<in TIn, TOut>
    {
        Task<TOut> WorkAsync(TIn input);
    }

    public abstract class ComponentBase
    {
        private static DateTime _now = DateTime.Now;

        public static DateTime Now => _now;
#if DEBUG
        internal static void SetNow(DateTime now) => _now = now;
#endif

        protected static void Break() => throw new BreakException();
    }

    public abstract class ComponentBase<T> :
        ComponentBase,
        IComponent<T, T>
        where T : class
    {
        public async Task<T> WorkAsync(T input)
        {
            Debug.Assert(input != null);
            await OnWorkAsync(input);
            return input;
        }

        protected abstract Task OnWorkAsync(T input);
    }

    public abstract class ComponentBase<TIn, TOut> :
        ComponentBase,
        IComponent<TIn, TOut>
        where TIn : class
        where TOut : new()
    {
        public async Task<TOut> WorkAsync(TIn input)
        {
            Debug.Assert(input != null);
            var output = new TOut();
            (output as Cfg.IInitialize<TIn>)?.Init(input);
            await OnWorkAsync(input, output);
            return output;
        }

        protected abstract Task OnWorkAsync(TIn input, TOut output);
    }

    public static class Runner
    {
        public static Func<TIn, Task<TOut>> Start<TIn, TOut>(
            Func<IComponent<TIn, TOut>> creator)
        {
            Debug.Assert(creator != null);
            return input =>
            {
                return creator
                    .CallWithLogInfoAsync(_ => _.WorkAsync(input));
            };
        }

        public static Func<TIn, Task<TOut>> Then<TIn, TOut, T>(
            this Func<TIn, Task<T>> @this,
            Func<IComponent<T, TOut>> creator)
        {
            Debug.Assert(creator != null);
            return async input =>
            {
                var tmp = await @this(input);
                return await creator
                    .CallWithLogInfoAsync(_ => _.WorkAsync(tmp));
            };
        }

        public static Func<TIn, Task<TOut>> WithCatchBreak<TIn, TOut>(this Func<TIn, Task<TOut>> @this)
        {
            Debug.Assert(@this != null);
            return async input =>
            {
                try
                {
                    return await @this(input);
                }
                catch (BreakException)
                {
                    return default;
                }
            };
        }

        public static Task<TOut> RunAsync<TIn, TOut>(
            this Func<TIn, Task<TOut>> @this,
            TIn @params)
        {
            Debug.Assert(@this != null);
            return @this
                .WithCatchBreak()
                .WithLogStartStop()
                .Invoke(@params);
        }
    }
}