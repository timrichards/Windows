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
            return (source.Count == 0);
        }

        internal static bool IsEmpty(this ListView.ListViewItemCollection source)
        {
            return (source.Count == 0);
        }

        internal static bool IsEmpty(this ListView.SelectedListViewItemCollection source)
        {
            return (source.Count == 0);
        }

        internal static bool IsEmpty(this TreeNodeCollection source)
        {
            return (source.Count == 0);
        }

        internal static bool IsEmptyA(this System.Collections.IList source)
        {
            return (source.Count == 0);
        }

        internal static void First<T>(this IEnumerable<T> source, Action<T> action)
        {
            source
                .FirstOrDefault(item =>
            {
                action(item);
                return true;
            });
        }

        internal static void FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            First(source, action);

#if (DEBUG && false)
            var enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
                MBoxStatic.Assert(0, false == enumerator.MoveNext());
#endif
        }

        internal static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            source
                .All(item => { action(item); return true; });
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

            var s = string
                .Join("", source.ToString()
                .Where(c => Char.IsControl(c) == false))
                .Trim();

            return (s.Length > 0) ? s : null;
        }
    }
}
