using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    static public class ExtensionMethodsStatic_Public
    {
        static public IDisposable LocalSubscribe<T>(this IObservable<T> source, decimal nLocation, Action<T> onNext)
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
                    Util.Assert(nLocation, false, "Exception in LocalSubscribe\n" +
                        e.GetBaseException().Message);
                }
            });
        }

        static readonly IDictionary<int, Tuple<DateTime, WeakReference>> _lsSubjects = new Dictionary<int, Tuple<DateTime, WeakReference>>();
        static public void LocalOnNext<T>(this LocalSubject<T> subject, T value, int nOnNextAssertLoc, int nInitiator = 0)
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
    }

    static partial class ExtensionMethodsStatic
    {
        static internal bool FileExists(this string strFile)
        {
            if (null == strFile)
                return false;

            if (2 > strFile.Length)
                return Statics.IsoStore.FileExists(strFile);

            return
                (':' == strFile[1])
                ? File.Exists(strFile)
                : Statics.IsoStore.FileExists(strFile);
        }

        static internal string
            FileMoveToIso(this string strSource, string strIsoDest)
        {
            if (null == strSource)
                return null;

            if (2 > strSource.Length)
            {
                Statics.IsoStore.MoveFile(strSource, strIsoDest);
                return strIsoDest;
            }

            if (':' == strSource[1])
                MoveFile_(strSource, strIsoDest);
            else
                Statics.IsoStore.MoveFile(strSource, strIsoDest);

            return strIsoDest;
        }

        static void
            MoveFile_(string strSource, string strDest)
        {
            using (var sr = File.OpenText(strSource))
            using (var sw = new StreamWriter(Statics.IsoStore.CreateFile(Statics.TempPathIso + Path.GetFileName(strDest))))
                Util.CopyStream(sr, sw);

            File.Delete(strSource);
        }

        static internal IEnumerable<string> ReadLines(this string strFile)
        {
            if (null == strFile)
                return new string[0];

            if (2 > strFile.Length)
                return ReadLines_(strFile);

            return
                (':' == strFile[1])
                ? File.ReadLines(strFile)
                : ReadLines_(strFile);
        }

        static IEnumerable<string>
            ReadLines_(string path)
        {
            var nCount = 0;

            using (var fs = new StreamReader(Statics.IsoStore.OpenFile(path, FileMode.Open)))
            {
                string strLine = null;

                while (null != (strLine = fs.ReadLine()))
                {
                    ++nCount;
                    yield return strLine;
                }
            }

            Util.WriteLine("" + nCount + " " + path);
        }
    }

    static partial class ExtensionMethodsStatic
    {
        static internal T As<T>(this object o) where T: class => (o is T) ? (T)o : null;  // 20x faster than as p. 123

        static internal T FirstOnlyAssert<T>(this IEnumerable<T> source)
        {
            var retVal = source.FirstOrDefault();
#if (DEBUG)
            var enumerator = source.GetEnumerator();    // GetEnumerator() is only used here 3x and ListViewVM_Base<T> 2x 7/28/15

            if (enumerator.MoveNext())
                Util.Assert(99903, false == enumerator.MoveNext());
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
            var enumerator = source.GetEnumerator();    // GetEnumerator() is only used here 3x and ListViewVM_Base<T> 2x 7/28/15

            if (enumerator.MoveNext())
                Util.Assert(99953, false == enumerator.MoveNext());
#endif
            return bRetVal;
        }

        // List has its own ForEach
        static internal void ForEach<T>(this IEnumerable<T> source, Action<T> action)           // 8 references on 7/23/15
        {
            if (null == source)
                return;

            foreach (var item in source)
                action(item);
        }

        static internal TMember
            Get<THolder, TMember>(this WeakReference<THolder> wr, Func<THolder, TMember> getValue)
            where THolder : class
        {
            THolder holder = null;

            wr.TryGetTarget(out holder);

            return
                (null != holder)
                ? getValue(holder)
                : default(TMember);
        }

        static internal bool HasExactly<T>(this IEnumerable<T> source, int nDesiredElements)    // 1 reference on 7/6/15
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

        static internal string ToPrintString(this object source)
        {
            if (null == source)
                return null;

            var s =
                string.Join("",
                ("" + source).Where(c => Char.IsControl(c) == false))
                .Trim();

            return (0 < s.Length) ? s : null;
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

        static internal T2 TryGetValue<T1, T2>(this SortedDictionary<T1, T2> dict, T1 key) where T2 : class
        {
            T2 outValue = null;

            dict.TryGetValue(key, out outValue);
            return outValue;
        }

        static internal T2 TryGetValue<T1, T2>(this ConcurrentDictionary<T1, T2> dict, T1 key) where T2 : class
        {
            T2 outValue = null;

            dict.TryGetValue(key, out outValue);
            return outValue;
        }

        static internal DateTime ToDateTime(this string str)
        {
            var nRet = DateTime.MinValue;

            if (false == DateTime.TryParse(str, out nRet))
                Util.Assert(99925, false);

            return nRet;
        }

        static internal int ToInt(this string str)
        {
            var nRet = 0;

            if (false == int.TryParse(str, out nRet))
                Util.Assert(99930, false);

            return nRet;
        }

        static internal ulong ToUlong(this string str)
        {
            var nRet = 0UL;

            if (false == ulong.TryParse(str, out nRet))
                Util.Assert(99929, false);

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
            if (null == window)
                return window;

            window.Left = r.X;
            window.Top = r.Y;
            window.Width = r.Width;
            window.Height = r.Height;
            return window;
        }
    }
}
