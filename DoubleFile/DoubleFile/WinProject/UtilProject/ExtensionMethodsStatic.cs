using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;

namespace DoubleFile
{
    static internal partial class ExtensionMethodsStatic
    {
        static Dictionary<int, DateTime> _lsSubjects = new Dictionary<int, DateTime>();
        static internal bool LocalOnNext<T>(this Subject<T> subject, T value, int nOnNextAssertLoc)
        {
            var dt = DateTime.MinValue;

            _lsSubjects.TryGetValue(nOnNextAssertLoc, out dt);

            if ((null != dt) &&
                (DateTime.Now - dt) < TimeSpan.FromMilliseconds(100))
            {
                MBoxStatic.Assert(nOnNextAssertLoc, false);
                return false;
            }

            _lsSubjects[nOnNextAssertLoc] = DateTime.Now;
            subject.OnNext(value);
            return true;
        }

        static internal bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return (false == source.Any());
        }

        static internal bool IsEmpty<T>(this ICollection<T> source)
        {
            return (source.Count == 0);
        }

        static internal bool IsEmpty<T1, T2>(this IDictionary<T1, T2> source)
        {
            return (source.Count == 0);
        }

        static internal bool IsEmptyA(this System.Collections.IList source)
        {
            return (source.Count == 0);
        }

        static internal bool IsEmpty<T>(this IList<T> source)
        {
            return (source.Count == 0);
        }

        static internal bool IsEmpty(this ListView.ListViewItemCollection source)
        {
            return (source.Count == 0);
        }

        static internal bool IsEmpty(this ListView.SelectedListViewItemCollection source)
        {
            return (source.Count == 0);
        }

        static internal bool IsEmpty<T>(this Stack<T> source)
        {
            return (source.Count == 0);
        }

        static internal bool IsEmpty(this TreeNodeCollection source)
        {
            return (source.Count == 0);
        }

        static internal void First<T>(this IEnumerable<T> source, Action<T> action)
        {
            source
                .FirstOrDefault(item =>
            {
                action(item);
                return true;
            });
        }

        static internal void FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            First(source, action);

#if (DEBUG)
            var enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
                MBoxStatic.Assert(99953, false == enumerator.MoveNext());
#endif
        }

        static internal void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            source
                .All(item => { action(item); return true; });
        }

        static internal bool HasOnlyOne(this System.Collections.IList source)
        {
            return (source.Count == 1);
        }
    }

    static internal partial class ExtensionMethodsStatic
    {
        static internal bool ContainsKeyA<T1, T2>(this IDictionary<T1, T2> dict, T1 key)
        {
            T2 outValue;

            return dict.TryGetValue(key, out outValue);
        }

        static internal bool ContainsKeyB<T1, T2>(this IReadOnlyDictionary<T1, T2> dict, T1 key)
        {
            T2 outValue;

            return dict.TryGetValue(key, out outValue);
        }
    }

    static internal partial class ExtensionMethodsStatic
    {
        static internal string ToPrintString(this object source)
        {
            if (source == null) return null;

            var s =
                string.Join("",
                    ("" + source).Where(c => Char.IsControl(c) == false))
                .Trim();

            return (s.Length > 0) ? s : null;
        }
    }
}
