using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    internal static partial class ExtensionMethodsStatic
    {
        internal struct dt
        {
            internal TimeSpan CountLE;
            internal TimeSpan CountEQ;
        }

        static internal dt[] atsDT = new dt[5];

        internal static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return (false == source.GetEnumerator().MoveNext());
        }

        internal static bool IsEmpty<T>(this UList<T> source)
        {
            var dtCount = DateTime.Now;
            var bRetA = (source.Count <= 0);
            atsDT[0].CountLE += DateTime.Now - dtCount;

            var dtEnumerator = DateTime.Now;
            var bRetB = (source.Count == 0);
            atsDT[0].CountEQ += DateTime.Now - dtEnumerator;

            return bRetA;
        }

        internal static bool IsEmpty(this ListView.ListViewItemCollection source)
        {
            var dtCount = DateTime.Now;
            var bRetA = (source.Count <= 0);
            atsDT[1].CountLE += DateTime.Now - dtCount;

            var dtEnumerator = DateTime.Now;
            var bRetB = (source.Count == 0);
            atsDT[1].CountEQ += DateTime.Now - dtEnumerator;

            return bRetA;
        }

        internal static bool IsEmpty(this ListView.SelectedListViewItemCollection source)
        {
            var dtCount = DateTime.Now;
            var bRetA = (source.Count <= 0);
            atsDT[2].CountLE += DateTime.Now - dtCount;

            var dtEnumerator = DateTime.Now;
            var bRetB = (source.Count == 0);
            atsDT[2].CountEQ += DateTime.Now - dtEnumerator;

            return bRetA;
        }

        internal static bool IsEmpty(this TreeNodeCollection source)
        {
            var dtCount = DateTime.Now;
            var bRetA = (source.Count <= 0);
            atsDT[3].CountLE += DateTime.Now - dtCount;

            var dtEnumerator = DateTime.Now;
            var bRetB = (source.Count == 0);
            atsDT[3].CountEQ += DateTime.Now - dtEnumerator;

            return bRetA;
        }

        internal static bool IsEmptyA(this System.Collections.IList source)
        {
            var dtCount = DateTime.Now;
            var bRetA = (source.Count <= 0);
            atsDT[4].CountLE += DateTime.Now - dtCount;

            var dtEnumerator = DateTime.Now;
            var bRetB = (source.Count == 0);
            atsDT[4].CountEQ += DateTime.Now - dtEnumerator;

            return bRetA;
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
