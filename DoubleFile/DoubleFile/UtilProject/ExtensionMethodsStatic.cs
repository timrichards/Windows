using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    internal static partial class ExtensionMethodsStatic
    {
        internal static IEnumerator<T> FirstOnly<T>(this IEnumerable<T> source, Action<T> action)
        {
            var enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
                action(enumerator.Current);

            return enumerator;
        }

        internal static void FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            var enumerator = FirstOnly(source, action);
            MBoxStatic.Assert(0, false == enumerator.MoveNext());
        }

        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
    }

    internal static partial class ExtensionMethodsStatic
    {
        internal static string ToPrintString(this object source)
        {
            if (source == null) return null;

            string s = string.Join("", source.ToString().Cast<char>().Where(c => Char.IsControl(c) == false)).Trim();

            if (s.Length == 0) return null;                             // Returns null if empty

            return s;
        }
    }
}
