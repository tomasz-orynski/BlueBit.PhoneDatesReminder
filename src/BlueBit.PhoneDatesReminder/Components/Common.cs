using DefensiveProgrammingFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using BlueBit.PhoneDatesReminder.Commons.Extensions;
using Serilog;
using System.Runtime.CompilerServices;

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

        protected static IEnumerable<TimeSpan> RetrySleepDurations => new[] {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(5)
        };

        protected async Task<TOut> WithLogingAsync<TOut>(Func<Task<TOut>> func, [CallerMemberName]string name = null)
        {
            func.CannotBeNull();
            name.CannotBeNull();

            var type = GetType();
            var typeArgs = type.GetGenericArguments();
            var className = $"{type.Name}<{string.Join(",", typeArgs.Select(@_=>@_.Name))}>";

            var log = Log.ForContext(type);
            log.Information(">>{Class}.{Mehod}", className, name);
            try
            {
                return await func();
            }
            finally
            {
                log.Information("<<{Class}.{Mehod}", className, name);
            }
        }
    }

    public abstract class ComponentBase<T> :
        ComponentBase,
        IComponent<T, T>
        where T : class
    {
        public async Task<T> WorkAsync(T input)
        {
            input.CannotBeNull();
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
        public Task<TOut> WorkAsync(TIn input) => WithLogingAsync(async () =>
        {
            input.CannotBeNull();
            var output = new TOut();
            (output as Cfg.IInitialize<TIn>)?.Init(input);
            await OnWorkAsync(input, output);
            return output;
        });

        protected abstract Task OnWorkAsync(TIn input, TOut output);
    }

    public static class Runner
    {
        public static Func<TIn, Task<TOut>> Start<TIn, TOut>(
            Func<IComponent<TIn, TOut>> creator)
        {
            creator.CannotBeNull();
            return async input =>
            {
                var obj = creator();
                obj.CannotBeNull();
                return await obj.WorkAsync(input);
            };
        }

        public static Func<TIn, Task<TOut>> Then<TIn, TOut, T>(
            this Func<TIn, Task<T>> @this,
            Func<IComponent<T, TOut>> creator)
        {
            creator.CannotBeNull();
            return async input =>
            {
                var tmp = await @this(input);
                var obj = creator();
                obj.CannotBeNull();
                return await obj.WorkAsync(tmp);
            };
        }

        public static Func<TIn, Task<T>> ThenMany<TIn, T>(
            this Func<TIn, Task<T>> @this,
            params Func<IComponent<T, T>>[] @params)
        {
            @params.CannotBeNull();
            @params.CannotBeEmpty();
            @params.CannotContainNull();
            return async input =>
            {
                var tmp = await @this(input);
                var tasks = @params
                    .Select(async creator =>
                    {
                        var obj = creator();
                        obj.CannotBeNull();
                        await obj.WorkAsync(tmp);
                    })
                    .ToArray();
                await Task.WhenAll(tasks);
                return tmp;
            };
        }

        public static Func<TIn, Task<TOut>> WithCatchBreak<TIn, TOut>(this Func<TIn, Task<TOut>> @this)
        {
            @this.CannotBeNull();
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

        public static async Task<TOut> RunAsync<TIn, TOut>(
            this Func<TIn, Task<TOut>> @this,
            TIn @params)
        {
            @this.CannotBeNull();
            return await @this
                .WithCatchBreak()
                .WithLogStartStop()
                .Invoke(@params);
        }
    }
}