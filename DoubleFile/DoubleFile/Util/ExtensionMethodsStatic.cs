using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    static public class ExtensionMethodsStatic_Public
    {
        static public IDisposable
            LocalSubscribe<T>(this IObservable<T> source, decimal nLocation, Action<T> onNext)
        {
            return
                source.Subscribe(t =>
            {
                try
                {
                    onNext(t);
                }
                catch (Exception e)
                {
                    var b = e.GetBaseException();

                    Util.Assert(nLocation, false, b.GetType() + " in LocalSubscribe\n" +
                        b.Message + "\n" + b.StackTrace);
                }
            });
        }

        static public void
            LocalOnNext<T>(this LocalSubject<T> subject, T value, int nOnNextAssertLoc, int nInitiator = 0)
        {
            Util.Assert(nOnNextAssertLoc, 0 <= nInitiator);

            if (0 == nInitiator)
                nInitiator = nOnNextAssertLoc;

            var o = _lsSubjects.TryGetValue(nOnNextAssertLoc);
            var oldValue = o?.Item2?.Target ?? default(T);

            if ((null == o) ||
                (false == (oldValue?.Equals(value) ?? false)) ||
                (DateTime.Now - o.Item1) > TimeSpan.FromMilliseconds(100))
            {
                _lsSubjects[nOnNextAssertLoc] = Tuple.Create(DateTime.Now, new WeakReference(value));
                Util.ThreadMake(() => subject.OnNext(Tuple.Create(value, nInitiator)));
            }
            else if ((nOnNextAssertLoc < 99830) ||      // 99830 block is reserved: do not alert LocalOnNext
                (nOnNextAssertLoc >= 99840))
            {
                Util.Assert(nOnNextAssertLoc, false);
            }
        }

        static readonly IDictionary<int, Tuple<DateTime, WeakReference>>
            _lsSubjects = new ConcurrentDictionary<int, Tuple<DateTime, WeakReference>>();
    }

    static partial class ExtensionMethodsStatic
    {
        static internal T
            As<T>(this object o) where T: class => (o is T) ? (T)o : null;  // 20x faster than as p. 123

        static internal int
            LocalCompare(this string a, string b) => a.CompareTo(b);
          //  string.Compare(a, b, StringComparison.Ordinal);

        static internal T
            FirstOnlyAssert<T>(this IEnumerable<T> source)
        {
            var retVal = source.FirstOrDefault();
#if (DEBUG)
            var enumerator = source.GetEnumerator();    // GetEnumerator() is only used here 3x and ListViewVM_Base<T> 2x 7/28/15

            if (enumerator.MoveNext())
                Util.Assert(99903, false == enumerator.MoveNext());
#endif
            return retVal;
        }

        static internal bool
            FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            var bRetVal = false;

            source.FirstOrDefault(item =>
            {
                action(item);
                bRetVal = true;
                return true;    // from lambda; NOT a no-op: has to be a non-default value.
            });
#if (DEBUG)
            var enumerator = source.GetEnumerator();    // GetEnumerator() is only used here 3x and ListViewVM_Base<T> 2x 7/28/15

            if (enumerator.MoveNext())
                Util.Assert(99953, false == enumerator.MoveNext());
#endif
            return bRetVal;
        }

        // List has its own ForEach
        static internal void
            ForEach<T>(this IEnumerable<T> source, Action<T> action)           // 8 references on 7/23/15
        {
            if (null == source)
                return;

            foreach (var item in source)
                action(item);
        }

        static internal TMember
            Get<THolder, TMember>(this WeakReference<THolder> wr, Func<THolder, TMember> getValue) where THolder : class
        {
            THolder holder = null;

            wr.TryGetTarget(out holder);

            return
                (null != holder)
                ? getValue(holder)
                : default(TMember);
        }

        static internal bool
            HasExactly<T>(this IEnumerable<T> source, int nDesiredElements)    // 1 reference on 7/6/15
        {
            if (source is ICollection<T>)
            {
                Util.Assert(99890, false, bTraceOnly: true);
                return ((ICollection<T>)source).Count == nDesiredElements;
            }

            var ie = source.GetEnumerator();    // GetEnumerator() is only used here 3x and ListViewVM_Base<T> 2x 7/28/15

            for (var i = 0; i < nDesiredElements; ++i)
            {
                if (false == ie.MoveNext())
                    return false;
            }

            return false == ie.MoveNext();
        }

        static internal string
            ToPrintString(this object source)
        {
            if (null == source)
                return null;

            var s =
                string.Join("",
                ("" + source).Where(c => Char.IsControl(c) == false))
                .Trim();

            return (0 < s.Length) ? s : null;
        }

        static internal T2
            TryGetValue<T1, T2>(this IReadOnlyDictionary<T1, T2> dict, T1 key)
        {
            var outValue = default(T2);

            dict.TryGetValue(key, out outValue);
            return outValue;
        }

        static internal T2
            TryGetValue<T1, T2>(this IDictionary<T1, T2> dict, T1 key)
        {
            var outValue = default(T2);

            dict.TryGetValue(key, out outValue);
            return outValue;
        }

        static internal T2
            TryGetValue<T1, T2>(this SortedDictionary<T1, T2> dict, T1 key)
        {
            var outValue = default(T2);

            dict.TryGetValue(key, out outValue);
            return outValue;
        }

        static internal T2
            TryGetValue<T1, T2>(this ConcurrentDictionary<T1, T2> dict, T1 key)
        {
            var outValue = default(T2);

            dict.TryGetValue(key, out outValue);
            return outValue;
        }

        static internal DateTime
            ToDateTime(this string str)
        {
            var nRet = DateTime.MinValue;

            if (false == DateTime.TryParse(str, out nRet))
                Util.Assert(99925, false);

            return nRet;
        }

        static internal int
            ToInt(this string str)
        {
            var nRet = 0;

            if (false == int.TryParse(str, out nRet))
                Util.Assert(99930, false);

            return nRet;
        }

        static internal ulong
            ToUlong(this string str)
        {
            var nRet = 0UL;

            if (false == ulong.TryParse(str, out nRet))
                Util.Assert(99929, false);

            return nRet;
        }

        static internal Rect
            Scale(this Rect rc_in, double scale)
        {
            Rect rc = rc_in;

            rc.X *= scale;
            rc.Y *= scale;
            rc.Width *= scale;
            rc.Height *= scale;
            return rc;
        }

        static internal Rect
            GetRect(this Window window)
        {
            return new Rect(window.Left, window.Top, window.Width, window.Height);
        }

        static internal T
            SetRect<T>(this T window, Rect r) where T : Window
        {
            if (null == window)
                return window;

            window.Left = r.X;
            window.Top = r.Y;
            window.Width = r.Width;
            window.Height = r.Height;
            return window;
        }

        static internal double
            Right(this Rect r) => r.X + r.Width;
        static internal double
            Bottom(this Rect r) => r.Y + r.Height;
    }
}
