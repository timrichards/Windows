﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace DoubleFile
{
    static internal partial class ExtensionMethodsStatic
    {
        static IDictionary<int, Tuple<DateTime, WeakReference>> _lsSubjects = new Dictionary<int, Tuple<DateTime, WeakReference>>();
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
                Util.ThreadMake(() => subject.OnNext(Tuple.Create(value, nInitiator)));
            }
            else if ((nOnNextAssertLoc < 99830) ||      // 99830 block is reserved: do not alert LocalOnNext
                (nOnNextAssertLoc >= 99840))
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

        static internal bool First<T>(this IEnumerable<T> source, Action<T> action)
        {
            bool bRetVal = false;

            source.FirstOrDefault(item =>   // 6/9/15 This is the only use of FirstOrDefault.
            {
                action(item);
                bRetVal = true;
                return true;    // from lambda; NOT a no-op: has to be a non-default value.
            });

            return bRetVal;
        }

        static internal bool FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            var retVal = First(source, action);

#if (DEBUG)
            var enumerator = source.GetEnumerator();

            if (enumerator.MoveNext())
                MBoxStatic.Assert(99953, false == enumerator.MoveNext());
#endif
            return retVal;
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

        static internal string ToPrintString(this object source)
        {
            if (source == null) return null;

            var s =
                string.Join("",
                    ("" + source).Where(c => Char.IsControl(c) == false))
                .Trim();

            return (s.Length > 0) ? s : null;
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
