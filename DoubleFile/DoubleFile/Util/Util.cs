using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Linq;

namespace DoubleFile
{
    partial class Util : FileParse
    {
        static internal void Block(int nMilliseconds) { Block(new TimeSpan(0, 0, 0, 0, nMilliseconds)); }
        static internal void Block(TimeSpan napTime)
        {
            var blockingFrame = new DispatcherFrame(true) { Continue = true };

            ThreadMake(() =>
            {
                Thread.Sleep(napTime);
                blockingFrame.Continue = false;
            });

            Dispatcher.PushFrame(blockingFrame);
        }

        static internal void Closure(Action action) { action(); }
        static internal T Closure<T>(Func<T> action) { return action(); }

        static internal string DecodeAttributes(string strAttr)
        {
            var nAttr = default(FileAttributes);

            try
            {
                nAttr = (FileAttributes)Convert.ToInt32(strAttr, 16);
            }
            catch (ArgumentException)
            {
                MBoxStatic.Assert(99933, false);
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

            if (str.Length == 0)
                str = strAttr;
            else
                str += " (" + strAttr + ")";

            return str;
        }

        static internal string FormatSize(string in_str, bool bBytes = false)
        {
            ulong retVal = 0;

            bool bSuccess = ulong.TryParse(in_str ?? "0", out retVal);

            MBoxStatic.Assert(99935, bSuccess);
            return FormatSize(retVal, bBytes);
        }

        static internal string FormatSize(ulong nLength, bool bBytes = false, bool bNoDecimal = false)
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
                return strSz + (bBytes ?
                    (" (" + nLength.ToString("###,###,###,###,###") + " bytes)") :
                    string.Empty);
            }

            return "0 bytes";
        }

        static internal TMember WR<THolder, TMember>(WeakReference<THolder> wr, Func<THolder, TMember> getValue)
            where THolder : class
            where TMember : class
        {
            THolder holder = null;

            wr.TryGetTarget(out holder);

            return
                (null != holder)
                ? getValue(holder)
                : null;
        }

        static internal string Localized(string key)
        {
            if (null == App.Current)
                return null;

            return "" + App.Current.Resources[key];
        }

        static internal void LocalDispose(IEnumerable<IDisposable> ieDisposable)
        {
            var lsDisposable = ieDisposable.ToList();
            var cts = new CancellationTokenSource();

            var thread = ThreadMake(() =>
            {
                Parallel.ForEach(lsDisposable, new ParallelOptions { CancellationToken = cts.Token },
                    d => d.Dispose());
            });

            Observable.Timer(TimeSpan.FromMilliseconds(250)).Timestamp()
                .Subscribe(x => cts.Cancel());

            Observable.Timer(TimeSpan.FromMilliseconds(500)).Timestamp()
                .Subscribe(x => thread.Abort());
        }

        static internal Thread ThreadMake(ThreadStart doSomething)
        {
            var thread = new Thread(doSomething) { IsBackground = true };

            thread.Start();
            return thread;
        }

        static internal void UIthread(Action action, bool bBlock = true)
        {
            if (App.LocalExit ||
                (false == App.LocalMainWindow is Window) ||
                App.LocalMainWindow.LocalIsClosed)
            {
                return;
            }

            var mainWindow = (Window)App.LocalMainWindow;

            if ((null == mainWindow) ||
                (null == mainWindow.Dispatcher) ||
                mainWindow.Dispatcher.HasShutdownStarted ||
                mainWindow.Dispatcher.HasShutdownFinished)
            {
                return;
            }

            try
            {
                if (mainWindow.Dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    var blockingFrame = new DispatcherFrame(true) { Continue = bBlock };

                    mainWindow.Dispatcher.Invoke(() =>
                    {
                        action();
                        blockingFrame.Continue = false;
                    });

                    Dispatcher.PushFrame(blockingFrame);
                }
            }
            catch (TaskCanceledException) { }
        }

        static internal void Write(string str)
        {
#if (DEBUG)
            Console.Write(str);
#endif
        }

        static internal void WriteLine(string str = null)
        {
#if (DEBUG)
            System.Console.WriteLine(str);
#endif
        }
    }
}
