using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    internal enum MoreThanOneEnum
    {
        Zero,
        One,
        MoreThanOne
    }

    static internal partial class ExtensionMethodsStatic
    {
        static IDictionary<int, Tuple<DateTime, WeakReference>> _lsSubjects = new Dictionary<int, Tuple<DateTime, WeakReference>>();
        static internal void LocalOnNext<T>(this LocalSubject<T> subject, T value, int nOnNextAssertLoc, int nInitiator = 0)
        {
            MBoxStatic.Assert(nOnNextAssertLoc, 0 <= nInitiator);

            if (0 == nInitiator)
                nInitiator = nOnNextAssertLoc;

            var o = _lsSubjects.TryGetValue(nOnNextAssertLoc);
            var oldValue = default(T);

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
                Util.ThreadMake(() => subject.OnNext(Tuple.Create(value, nInitiator)));
            }
            else if ((nOnNextAssertLoc < 99830) ||      // 99830 block is reserved: do not alert LocalOnNext
                (nOnNextAssertLoc >= 99840))
            {
                MBoxStatic.Assert(nOnNextAssertLoc, false);
            }
        }

        static internal T FirstOnlyAssert<T>(this IEnumerable<T> source)
        {
            var retVal = source.FirstOrDefault();

#if (DEBUG)
            var enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
                MBoxStatic.Assert(99903, false == enumerator.MoveNext());
#endif
            return retVal;
        }

        static internal bool FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            var bRetVal = false;

            source.FirstOrDefault(item =>
            {
                action(item);
                bRetVal = true;
                return true;    // from lambda; NOT a no-op: has to be a non-default value.
            });

#if (DEBUG)
            var enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
                MBoxStatic.Assert(99953, false == enumerator.MoveNext());
#endif
            return bRetVal;
        }

        static internal void ForEach<T>(this IEnumerable<T> source, Action<T> action)           // 3 references on 7/6/15
        {
            foreach (var item in source)
                action(item);
        }

        static internal bool HasExactly<T>(this ICollection<T> source, int nDesiredElements)    // 5 references on 7/6/15
        {
            return source.Count == nDesiredElements;
        }

        static internal bool HasExactly<T>(this IEnumerable<T> source, int nDesiredElements)    // 1 reference on 7/6/15
        {
            if (source is ICollection<T>)
            {
                MBoxStatic.Assert(99890, false, bTraceOnly: true);
                return ((ICollection<T>)source).HasExactly(nDesiredElements);
            }

            var ie = source.GetEnumerator();

            for (var i = 0; i < nDesiredElements; ++i)
            {
                if (false == ie.MoveNext())
                    return false;
            }

            return false == ie.MoveNext();
        }

        static internal bool LocalAny<T>(this ICollection<T> source)                    // 32 references on 7/6/15
        {
            return 0 < source.Count;
        }

        static internal bool LocalAny<T>(this IEnumerable<T> source)                    // 9 references on 7/6/15
        {
            if (source is ICollection<T>)
            {
              //  MBoxStatic.Assert(99889, false, bTraceOnly: true);                    // Collate._ieRootNodes 7/6/15
                return 0 < ((ICollection<T>)source).Count;
            }

            return source.Any();
        }

        static internal MoreThanOneEnum MoreThanOne<T>(this ICollection<T> source)      // 5 references on 7/6/15
        {
            return
                (0 == source.Count)
                ? MoreThanOneEnum.Zero
                : (1 == source.Count)
                ? MoreThanOneEnum.One
                : MoreThanOneEnum.MoreThanOne;
        }

        static internal MoreThanOneEnum MoreThanOne<T>(this IEnumerable<T> source)      // not used as of 7/6/15
        {
            if (source is ICollection<T>)
            {
                MBoxStatic.Assert(99888, false, bTraceOnly: true);
                return ((ICollection<T>)source).MoreThanOne();
            }

            var ie = source.GetEnumerator();

            if (false == ie.MoveNext())
                return MoreThanOneEnum.Zero;

            if (false == ie.MoveNext())
                return MoreThanOneEnum.One;

            return MoreThanOneEnum.MoreThanOne;
        }

        static internal string ToPrintString(this object source)
        {
            if (source == null) return null;

            var s =
                string.Join("",
                    ("" + source).Where(c => Char.IsControl(c) == false))
                .Trim();

            return (s.Length > 0) ? s : null;
        }

        static internal T2 TryGetValue<T1, T2>(this IReadOnlyDictionary<T1, T2> dict, T1 key) where T2 : class
        {
            T2 outValue = null;

            dict.TryGetValue(key, out outValue);
            return outValue;
        }

        static internal T2 TryGetValue<T1, T2>(this IDictionary<T1, T2> dict, T1 key) where T2 : class
        {
            T2 outValue = null;

            dict.TryGetValue(key, out outValue);
            return outValue;
        }

        static internal DateTime ToDateTime(this string str)
        {
            DateTime nRet = DateTime.MinValue;

            if (false == DateTime.TryParse(str, out nRet))
                MBoxStatic.Assert(99925, false);

            return nRet;
        }

        static internal int ToInt(this string str)
        {
            int nRet = 0;

            if (false == int.TryParse(str, out nRet))
                MBoxStatic.Assert(99930, false);

            return nRet;
        }

        static internal ulong ToUlong(this string str)
        {
            ulong nRet = 0;

            if (false == ulong.TryParse(str, out nRet))
                MBoxStatic.Assert(99929, false);

            return nRet;
        }

        static internal RectangleF Scale(this Rectangle rc_in, SizeF scale)
        {
            RectangleF rc = rc_in;

            rc.X *= scale.Width;
            rc.Y *= scale.Height;
            rc.Width *= scale.Width;
            rc.Height *= scale.Height;
            return rc;
        }

        static internal Rect GetRect(this Window window)
        {
            return new Rect(window.Left, window.Top, window.Width, window.Height);
        }

        static internal T SetRect<T>(this T window, Rect r) where T : Window
        {
            window.Left = r.X;
            window.Top = r.Y;
            window.Width = r.Width;
            window.Height = r.Height;
            return window;
        }
    }
}
