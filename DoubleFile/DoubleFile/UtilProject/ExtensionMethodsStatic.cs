using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    internal static partial class ExtensionMethodsStatic
    {
        internal static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return (false == source.GetEnumerator().MoveNext());
        }

        internal static bool IsEmpty<T>(this UList<T> source)
        {
            return (source.Count <= 0);
        }

        internal static bool IsEmpty(this ListView.ListViewItemCollection source)
        {
            return (source.Count <= 0);
        }

        internal static bool IsEmpty(this ListView.SelectedListViewItemCollection source)
        {
            return (source.Count <= 0);
        }

        internal static bool IsEmpty(this TreeNodeCollection source)
        {
            return (source.Count <= 0);
        }

        internal static bool IsEmptyA(this System.Collections.IList source)
        {
            return (source.Count <= 0);
        }

        internal static IEnumerator<T> First<T>(this IEnumerable<T> source, Action<T> action)
        {
            var enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
                action(enumerator.Current);

            return enumerator;
        }

        internal static void FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            var enumerator = First(source, action);

            MBoxStatic.Assert(0, false == enumerator.MoveNext());
        }

        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            source
                .All(item => { action(item); return false; });
        }

        internal static bool HasOnlyOne(this System.Collections.IList source)
        {
            return (source.Count == 1);
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
