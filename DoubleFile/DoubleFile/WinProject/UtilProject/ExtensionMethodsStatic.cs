using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Windows.Forms;

namespace DoubleFile
{
    class LocalSubject<T> : ISubject<Tuple<T, int>>
    {
        public void OnCompleted() { _subject.OnCompleted(); }
        public void OnError(Exception error) { _subject.OnError(error); }
        public void OnNext(Tuple<T, int> value) { _subject.OnNext(value); }
        public IDisposable Subscribe(IObserver<Tuple<T, int>> observer) { return _subject.Subscribe(observer); }

        Subject<Tuple<T, int>>
            _subject = new Subject<Tuple<T, int>>();
    }

    static internal partial class ExtensionMethodsStatic
    {
        static Dictionary<int, Tuple<DateTime, WeakReference>> _lsSubjects = new Dictionary<int, Tuple<DateTime, WeakReference>>();
        static internal void LocalOnNext<T>(this LocalSubject<T> subject, T value, int nOnNextAssertLoc, int nInitiator = 0)
        {
            MBoxStatic.Assert(nOnNextAssertLoc, 0 <= nInitiator);

            if (0 == nInitiator)
                nInitiator = nOnNextAssertLoc;

            Tuple<DateTime, WeakReference> o = null;

            _lsSubjects.TryGetValue(nOnNextAssertLoc, out o);
            T oldValue = default(T);

            if ((null != o) &&
                (null != o.Item2) &&
                (null != o.Item2.Target))
            {
                oldValue = (T)o.Item2.Target;
            }

            if ((null == o) ||
                (null == oldValue) ||
                //EqualityComparer<T>.Default.Equals(oldValue) ||
                (false == oldValue.Equals(value)) ||
                (DateTime.Now - o.Item1) > TimeSpan.FromMilliseconds(100))
            {
                _lsSubjects[nOnNextAssertLoc] = Tuple.Create(DateTime.Now, new WeakReference(value));
                new Thread(() => subject.OnNext(Tuple.Create(value, nInitiator))).Start();
            }
            else
            {
                MBoxStatic.Assert(nOnNextAssertLoc, false);
            }
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
