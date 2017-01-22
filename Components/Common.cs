using System;
using System.Diagnostics;

namespace BlueBit.PhoneDatesReminder.Components
{
    public sealed class BreakException : Exception {}

    public sealed class Void {}

    public interface IComponent<in TIn, out TOut>
    {
        TOut Work(TIn input);
    }

    public abstract class ComponentBase<TIn, TOut> :
        IComponent<TIn, TOut>
        where TIn: class
        where TOut: new()
    {
        protected static void Break() { throw new BreakException(); }

        public TOut Work(TIn input)
        {
            Debug.Assert(input != null);
            var output = new TOut();
            (output as Cfg.IInitialize<TIn>)?.Init(input);
            OnWork(input, output);
            return output;
        }
        protected abstract void OnWork(TIn input, TOut output);
    }

    public static class Runner
    {
        public static Func<TIn, TOut> Start<TIn, TOut>(
            Func<IComponent<TIn, TOut>> creator)
        {
            Debug.Assert(creator != null);
            return input => {
                return creator
                    .CallWithLogInfo(_ => _.Work(input));
            };
        }
        public static Func<TIn, TOut> Then<TIn, TOut, T>(
            this Func<TIn, T> @this,
            Func<IComponent<T, TOut>> creator)
        {
            Debug.Assert(creator != null);
            return input => {
                var tmp = @this(input);
                return creator
                    .CallWithLogInfo(_ => _.Work(tmp));
            };
        }

        public static TOut Run<TIn, TOut>(
            this Func<TIn, TOut> @this,
            TIn @params)
        {
            Debug.Assert(@this != null);
            return @this
                .WithLogStartStop()
                .Invoke(@params);
        }
    }
}