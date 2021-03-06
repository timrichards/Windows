﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Reflection;

namespace DoubleFile
{
    public class UtilPublic
    {
        static public bool
            Assert(decimal nLocation, bool bCondition, string strError = null, bool bIfDefDebug = false) =>
            Util.Assert(nLocation, bCondition, strError, bIfDefDebug);
    }

    static class Util
    {
        static internal bool
            Assert(decimal nLocation, bool bCondition, string strError = null, bool bIfDefDebug = false)
        {
            if (bCondition)
                return true;
#if (false == DEBUG)
            if (bIfDefDebug)
                return false;
#endif
            MBoxStatic.Fail(nLocation, strError);
            return false;
        }

        static internal T
            AssertNotNull<T>(decimal nLocation, T t, string strError_in = null, bool bIfDefDebug = false)
        {
            Assert(nLocation, null != t, strError_in, bIfDefDebug);
            return t;
        }

        static internal void
            Block(int nMilliseconds) => Block(TimeSpan.FromMilliseconds(nMilliseconds));
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

            var str = DecodeAttributes(nAttr);

            if (0 == str.Length)
                str = strAttr;
            else
                str += " (" + strAttr + ")";

            return str;
        }

        static internal string
            DecodeAttributes(FileAttributes nAttr)
        {
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
            return str.TrimStart();
        }

        public static T
            GetDependencyObjectField<T>(Type dependencyObjectType, string dpName) where T : class
        {
            var fieldInfo = dependencyObjectType.GetField(dpName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (null == fieldInfo)
                return null;

            return fieldInfo.GetValue(null).As<T>();
        }

        static internal string
            FormatSize(this string in_str, bool bytes = false) =>
            FormatSize((in_str ?? "0").ToUlong(), bytes);

        static internal string
            FormatSize(this ulong nLength, bool bytes = false, bool bNoDecimal = false)
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
                    bytes
                    ? (" (" + nLength.ToString("###,###,###,###,###") + " bytes)")
                    : "");
            }

            return "0 bytes";
        }

        static internal T
            CreateJaggedArray<T>(params int[] lengths) => (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        static object InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            var array = Array.CreateInstance(type, lengths[index]);
            var elementType = type.GetElementType();

            if (null != elementType)
            {
                for (int i = 0; i < lengths[index]; i++)
                    array.SetValue(InitializeJaggedArray(elementType, index + 1, lengths), i);
            }

            return array;
        }

        static internal T[,]
            CreateRectangularArray<T>(IReadOnlyList<IReadOnlyList<T>> arrays)
        {
            var rowLength = arrays[0].Count;
            var retVal = new T[arrays.Count, rowLength];

            for (var i = 0; i < arrays.Count; i++)
            {
                for (var j = 0; j < rowLength; j++)
                    retVal[i, j] = arrays[i][j];
            }

            return retVal;
        }

        static internal string
            Localized(string key) => Application.Current?.Resources[key]?.ToString();

        static internal void
            LocalDispose(IEnumerable<IDisposable> ieDisposable)
        {
            var lsDisposable = ieDisposable.ToList();
            var cts = new CancellationTokenSource();

            IDisposable timer1 = null;
            IDisposable timer2 = null;

            var thread = ThreadMake(() =>
            {
                ParallelForEach(99653, lsDisposable, new ParallelOptions { CancellationToken = cts.Token },
                    d => d.Dispose());

                timer1?.Dispose();
                timer2?.Dispose();
            });

            timer1 = Observable.Timer(TimeSpan.FromMinutes(1)).Timestamp()
                .LocalSubscribe(99732, x => cts.Cancel());

            timer2 = Observable.Timer(TimeSpan.FromMinutes(2)).Timestamp()
                .LocalSubscribe(99731, x => thread.Abort());
        }

        static internal void
            ParallelForEach<TSource>(decimal nLocation, IEnumerable<TSource> source, Action<TSource> doSomething) =>
            ParallelForEach(nLocation, source, new ParallelOptions { }, doSomething);

        static internal void
            ParallelForEach<TSource>(decimal nLocation, IEnumerable<TSource> source, ParallelOptions options, Action<TSource> doSomething)
        {
#if (false == FOOBAR)
            try
            {
                Parallel.ForEach(source, options, s =>

#else
                foreach(var s in source)
#endif
                {
#if (false == FOOBAR)
                    try
                    {
#endif
                        doSomething(s);
#if (false == FOOBAR)
                    }
                    catch (ThreadAbortException) { }
                    catch (Exception e)
                    {
                        var b = e.GetBaseException();

                        Util.Assert(nLocation, false, b.GetType() + " in ParallelForEach\n" +
                            b.Message + "\n" + b.StackTrace);
                    }
#endif
                }
#if (false == FOOBAR)
                );
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
#endif
        }

        static internal Thread
            ThreadMake(ThreadStart doSomething)
        {
            var thread = new Thread(doSomething) { IsBackground = true };

            thread.Start();
            return thread;
        }

        static internal void
            UIthread(decimal nLocation, Action action)
        {
            if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                return;

            var blockingFrame = new LocalDispatcherFrame(nLocation);

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
