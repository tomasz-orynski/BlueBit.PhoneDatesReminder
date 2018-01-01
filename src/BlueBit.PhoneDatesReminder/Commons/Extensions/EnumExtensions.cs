using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BlueBit.PhoneDatesReminder.Commons.Extensions
{
    public static class Enum<T> where T : struct, IComparable, IConvertible, IFormattable //Enum
    {
        public static readonly IReadOnlyDictionary<string, T> Name2Value = BuildName2Value();
        public static readonly IReadOnlyDictionary<T, string> Value2Name = BuildValue2Name();

        private static Dictionary<string, T> BuildName2Value() => Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(_ => _.ToString(), _ => _);

        private static Dictionary<T, string> BuildValue2Name() => Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(_ => _, _ => _.ToString());
    }

    public static class EnumAttr<T, TAttr> 
        where T : struct, IComparable, IConvertible, IFormattable //Enum
        where TAttr: Attribute
    {
        public static readonly IReadOnlyDictionary<T, TAttr> Value2Attr = BuildValue2Attr();

        private static Dictionary<T, TAttr> BuildValue2Attr()
        {
            var type = typeof(T);
            var typeAttr = typeof(TAttr);
            return Enum<T>.Value2Name
                .ToDictionary(_ => _.Key, _ => (TAttr)type.GetField(_.Value).GetCustomAttributes(typeAttr, false)[0]);
        }
    }

    public static class EnumExtensions
    {
        public static string AsString<T>(this T @this) where T : struct, IComparable, IConvertible, IFormattable //Enum
            => Enum<T>.Value2Name[@this];

        public static string AsStringLower<T>(this T @this) where T : struct, IComparable, IConvertible, IFormattable //Enum
            => @this.AsString().ToLower();

        public static T AsEnum<T>(this string @this) where T : struct, IComparable, IConvertible, IFormattable //Enum
            => Enum<T>.Name2Value[@this];

        public static string AsDescription<T>(this T @this) where T : struct, IComparable, IConvertible, IFormattable //Enum
            => EnumAttr<T, DescriptionAttribute>.Value2Attr[@this].Description;
    }
}