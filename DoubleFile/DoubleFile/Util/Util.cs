using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

namespace DoubleFile
{
    public class UtilPublic
    {
        static public bool
            Assert(decimal nLocation, bool bCondition, string strError_in = null, bool bTraceOnly = false) =>
            Util.Assert(nLocation, bCondition, strError_in, bTraceOnly);
    }

    class Util : FileParse
    {
        static internal bool
            Assert(decimal nLocation, bool bCondition, string strError_in = null, bool bTraceOnly = false) =>
            MBoxStatic.Assert(nLocation, bCondition, strError_in, bTraceOnly);

        static internal T
            AssertNotNull<T>(decimal nLocation, T t, string strError_in = null, bool bTraceOnly = false)
        {
            Assert(nLocation, null != t, strError_in, bTraceOnly);
            return t;
        }

        static internal void
            Block(int nMilliseconds) => Block(new TimeSpan(0, 0, 0, 0, nMilliseconds));
        static internal void
            Block(TimeSpan napTime)
        {
            var blockingFrame = new LocalDispatcherFrame(99879);

            // One-shot: no need to dispose
            Observable.Timer(napTime).Timestamp()
                .Subscribe(x => blockingFrame.Continue = false);

            blockingFrame.PushFrameTrue();
        }

        static internal void
            Closure(Action action) => action();
        static internal T
            Closure<T>(Func<T> action) => action();

        static internal void
            CopyStream(StreamReader sr, StreamWriter sw, Func<char[], int, char[]> replace = null)
        {
            const int kBufSize = 1024 * 1024 * 4;
            var buffer = new char[kBufSize];
            var nRead = 0;

            if (null == replace)
                replace = (x, n) => x;

            while (0 < (nRead = sr.Read(buffer, 0, kBufSize)))
                sw.Write(replace(buffer, nRead), 0, nRead);
        }

        static internal string
            DecodeAttributes(string strAttr)
        {
            FileAttributes nAttr = 0;

            try
            {
                nAttr = (FileAttributes)Convert.ToInt32(strAttr, /* from base */ 16);
            }
            catch (ArgumentException)
            {
                Util.Assert(99933, false);
            }
            catch (FormatException)
            {
                Util.Assert(99920, false);
            }

            var str = "";

            if ((nAttr & FileAttributes.ReparsePoint) != 0) str += " ReparsePoint";
            if ((nAttr & FileAttributes.Normal) != 0) str += " Normal";
            if ((nAttr & FileAttributes.Hidden) != 0) str += " Hidden";
            if ((nAttr & FileAttributes.ReadOnly) != 0) str += " Readonly";
            if ((nAttr & FileAttributes.Archive) != 0) str += " Archive";
            if ((nAttr & FileAttributes.Compressed) != 0) str += " Compressed";
            if ((nAttr & FileAttributes.System) != 0) str += " System";
            if ((nAttr & FileAttributes.Temporary) != 0) str += " Tempfile";
            if ((nAttr & FileAttributes.Directory) != 0) str += " Directory";

            str = str.TrimStart();

            if (0 == str.Length)
                str = strAttr;
            else
                str += " (" + strAttr + ")";

            return str;
        }

        static internal string
            FormatSize(string in_str, bool bBytes = false) =>
            FormatSize((in_str ?? "0").ToUlong(), bBytes);

        static internal string
            FormatSize(ulong nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            var nT = nLength / 1024d / 1024 / 1024 / 1024 - .05;
            var nG = nLength / 1024d / 1024 / 1024 - .05;
            var nM = nLength / 1024d / 1024 - .05;
            var nK = nLength / 1024d - .05;     // Windows Explorer seems to not round
            const string kStrFmt_big = "###,##0.0";
            var strFormat = bNoDecimal ? "###,###" : kStrFmt_big;
            string strSz = null;

            if (((int)nT) > 0) strSz = nT.ToString(kStrFmt_big) + " TiB";
            else if (((int)nG) > 0) strSz = nG.ToString(kStrFmt_big) + " GiB";
            else if (((int)nM) > 0) strSz = nM.ToString(strFormat) + " MiB";
            else if (((int)nK) > 0) strSz = nK.ToString(strFormat) + " KiB";
            else strSz = "1 KiB";                    // Windows Explorer mins at 1K

            if (nLength > 0)
            {
                return strSz + (
                    bBytes
                    ? (" (" + nLength.ToString("###,###,###,###,###") + " bytes)")
                    : "");
            }

            return "0 bytes";
        }

        static internal T
            CreateJaggedArray<T>(params int[] lengths)
        {
            return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }

        static object
            InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            var array = Array.CreateInstance(type, lengths[index]);
            var elementType = type.GetElementType();

            if (elementType != null)
            {
                for (int i = 0; i < lengths[index]; i++)
                {
                    array.SetValue(
                        InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }

            return array;
        }

        static internal string
            Localized(string key) => App.Current?.Resources[key]?.ToString();

        static internal void
            LocalDispose(IEnumerable<IDisposable> ieDisposable)
        {
            var lsDisposable = ieDisposable.ToList();
            var cts = new CancellationTokenSource();

            var thread = ThreadMake(() =>
            {
                ParallelForEach(99653, lsDisposable, new ParallelOptions { CancellationToken = cts.Token },
                    d => d.Dispose());
            });

            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromMilliseconds(250)).Timestamp()
                .LocalSubscribe(99732, x => cts.Cancel());

            Observable.Timer(TimeSpan.FromMilliseconds(500)).Timestamp()
                .LocalSubscribe(99731, x => thread.Abort());
        }

        static internal void
            ParallelForEach<TSource>(decimal nLocation, IEnumerable<TSource> source, Action<TSource> doSomething) =>
            ParallelForEach(nLocation, source, new ParallelOptions { }, doSomething);

        static internal void
            ParallelForEach<TSource>(decimal nLocation, IEnumerable<TSource> source, ParallelOptions options, Action<TSource> doSomething)
        {
            try
            {
                Parallel.ForEach(source, options, s =>
                {
                    try
                    {
                        doSomething(s);
                    }
                    catch (ThreadAbortException) { }
                    catch (Exception e)
                    {
                        var b = e.GetBaseException();

                        Util.Assert(nLocation, false, b.GetType() + " in ParallelForEach\n" +
                            b.Message + "\n" + b.StackTrace);
                    }
                });
            }
            catch (AggregateException e)
            {
                var b = e.GetBaseException();

                Util.Assert(99669, false, "AggragateException in ParallelForEach\n" +
                    b.Message + "\n" + b.StackTrace);
            }
            catch (OperationCanceledException)
            {
            }
        }

        static internal Thread
            ThreadMake(ThreadStart doSomething)
        {
            var thread = new Thread(doSomething) { IsBackground = true };

            thread.Start();
            return thread;
        }

        static internal void
            UIthread(decimal nLocation, Action action, bool bBlock = true)
        {
            if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                return;

            var blockingFrame = new LocalDispatcherFrame(nLocation) { Continue = bBlock };

            try
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        action();
                        blockingFrame.Continue = false;     // 2
                    });

                    // fast operation (e.g. OnPropertyChanged()) may exit Invoke() before this line is even hit:
                    // 2 then 1 not the reverse.
                    if (blockingFrame.Continue)             // 1
                        blockingFrame.PushFrameTrue();
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                blockingFrame.Continue = false;
            }
        }

        static internal void
            Write(string str)
        {
#if (DEBUG)
            Console.Write(str);
#endif
        }

        static internal void
            WriteLine(string str = null)
        {
#if (DEBUG)
            Console.WriteLine(str);
#endif
        }
    }
}
