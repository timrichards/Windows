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

        internal struct dt
        {
            internal TimeSpan Count;
            internal TimeSpan Enumerator;
        }

        static internal dt[] atsDT = new dt[6];

        internal static bool IsEmpty<T>(this UList<T> source)
        {
            DateTime dtCount = DateTime.Now;
            bool bRetA = (source.Count <= 0);
            atsDT[0].Count += DateTime.Now - dtCount;

            DateTime dtEnumerator = DateTime.Now;
            bool bRetB = (false == source.GetEnumerator().MoveNext());
            atsDT[0].Enumerator += DateTime.Now - dtEnumerator;

            return bRetA;
        }

        internal static bool IsEmpty(this ListView.ListViewItemCollection source)
        {
            DateTime dtCount = DateTime.Now;
            bool bRetA = (source.Count <= 0);
            atsDT[1].Count += DateTime.Now - dtCount;

            DateTime dtEnumerator = DateTime.Now;
            bool bRetB = (false == source.GetEnumerator().MoveNext());
            atsDT[1].Enumerator += DateTime.Now - dtEnumerator;

            return bRetA;
        }

        internal static bool IsEmpty(this ListView.SelectedListViewItemCollection source)
        {
            DateTime dtCount = DateTime.Now;
            bool bRetA = (source.Count <= 0);
            atsDT[2].Count += DateTime.Now - dtCount;

            DateTime dtEnumerator = DateTime.Now;
            bool bRetB = (false == source.GetEnumerator().MoveNext());
            atsDT[2].Enumerator += DateTime.Now - dtEnumerator;

            return bRetA;
        }

        internal static bool IsEmpty(this TreeNodeCollection source)
        {
            DateTime dtCount = DateTime.Now;
            bool bRetA = (source.Count <= 0);
            atsDT[3].Count += DateTime.Now - dtCount;

            DateTime dtEnumerator = DateTime.Now;
            bool bRetB = (false == source.GetEnumerator().MoveNext());
            atsDT[3].Enumerator += DateTime.Now - dtEnumerator;

            return bRetA;
        }

        internal static bool IsEmptyA(this System.Collections.IList source)
        {
            DateTime dtCount = DateTime.Now;
            bool bRetA = (source.Count <= 0);
            atsDT[4].Count += DateTime.Now - dtCount;

            DateTime dtEnumerator = DateTime.Now;
            bool bRetB = (false == source.GetEnumerator().MoveNext());
            atsDT[4].Enumerator += DateTime.Now - dtEnumerator;

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
            DateTime dtCount = DateTime.Now;
            var bRetA = (source.Count == 1);
            atsDT[5].Count += DateTime.Now - dtCount;

            DateTime dtEnumerator = DateTime.Now;
            var enumerator = source.GetEnumerator();

            var bRetB = false;

            if (enumerator.MoveNext())
                bRetB = (false == enumerator.MoveNext());

            atsDT[5].Enumerator += DateTime.Now - dtEnumerator;

            return bRetA;
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
