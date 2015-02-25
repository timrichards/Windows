using System.Windows.Forms;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace DoubleFile
{
    class UtilAnalysis_DirList : FileParse
    {
        internal static void Closure(Action action) { action(); }
        internal static bool Closure(BoolAction action) { return action(); }

        internal static object UIthread(Control dispatcher, Action action, object[] args = null)
        {
            return UIthread(dispatcher, action as Delegate, args);
        }

        internal static object UIthread(Control dispatcher, Delegate action, object[] args = null)
        {
            if ((null == dispatcher) ||
                (dispatcher.IsDisposed))
            {
                return null;
            }

            try
            {
                if (dispatcher.InvokeRequired)
                {
                    return args == null
                        ? dispatcher.Invoke(action)
                        : dispatcher.Invoke(action, (object)args);
                }

                var action1 = action as Action;

                if (action1 != null)
                {
                    action1();
                }
                else
                {
                    var boolAction = action as BoolAction;

                    return (boolAction != null)
                        ? boolAction()
                        : action.DynamicInvoke(args);     // late-bound and slow
                }
            }
            catch (ObjectDisposedException e)
            {
                if (false == dispatcher.IsDisposed)
                {
                    MBoxStatic.Assert(99959, false, e.GetBaseException().Message);

#if (false == PUBLISH)
                    if (false == dispatcher.IsDisposed)
                        throw;
#endif
                }
            }
            catch (InvalidOperationException e)
            {
                MBoxStatic.Assert(99958, false, e.GetBaseException().Message);

#if (false == PUBLISH)
                if (false == dispatcher.IsDisposed)
                    throw;
#endif
            }

            return null;
        }

        internal static string DecodeAttributes(string strAttr)
        {
            var nAttr = (FileAttributes)Convert.ToInt32(strAttr, 16);
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

            if (str.Length == 0) str = strAttr;
            else str += " (" + strAttr + ")";

            return str;
        }

        internal static string FormatSize(string in_str, bool bBytes = false)
        {
            return FormatSize(ulong.Parse(in_str ?? "0"), bBytes);
        }

        internal static string FormatSize(ulong nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            var nT = nLength / 1024.0 / 1024.0 / 1024 / 1024 - .05;
            var nG = nLength / 1024.0 / 1024 / 1024 - .05;
            var nM = nLength / 1024.0 / 1024 - .05;
            var nK = nLength / 1024.0 - .05;     // Windows Explorer seems to not round
            const string kStrFmt_big = "###,##0.0";
            var strFormat = bNoDecimal ? "###,###" : kStrFmt_big;
            string strSz = null;

            if (((int)nT) > 0) strSz = nT.ToString(kStrFmt_big) + " TB";
            else if (((int)nG) > 0) strSz = nG.ToString(kStrFmt_big) + " GB";
            else if (((int)nM) > 0) strSz = nM.ToString(strFormat) + " MB";
            else if (((int)nK) > 0) strSz = nK.ToString(strFormat) + " KB";
            else strSz = "1 KB";                    // Windows Explorer mins at 1K

            if (nLength > 0)
            {
                return strSz + (bBytes ?
                    (" (" + nLength.ToString("###,###,###,###,###") + " bytes)") :
                    string.Empty);
            }

            return "0 bytes";
        }

        static internal void Write(string str)
        {
#if (DEBUG)
            Console.Write(str);
#endif
        }
    }
}
