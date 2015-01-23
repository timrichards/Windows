using System.Windows.Forms;
using System.Threading;         // release mode
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace DoubleFile
{
    delegate bool BoolAction();

    class UtilAnalysis_DirList : FileParse
    {
        internal static void Closure(Action action) { action(); }

        internal static object CheckAndInvoke(Control dispatcher, Delegate action, object[] args = null)
        {
            bool bInvoke = dispatcher.InvokeRequired;

            if (GlobalData.Instance.FormAnalysis_DirList_Closing)
            {
                return null;
            }

            try
            {
                if (bInvoke)
                {
                    if (args == null)
                    {
                        return dispatcher.Invoke(action);
                    }
                    else
                    {
                        return dispatcher.Invoke(action, (object)args);
                    }
                }
                else
                {
                    if (action is Action)
                    {
                        ((Action)action)();
                    }
                    else if (action is BoolAction)
                    {
                        return ((BoolAction)action)();
                    }
                    else
                    {
                        return action.DynamicInvoke(args);     // late-bound and slow
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                if (false == GlobalData.Instance.FormAnalysis_DirList_Closing)
                    throw;

                return null;
            }
            catch (InvalidOperationException)
            {
                if (false == GlobalData.Instance.FormAnalysis_DirList_Closing)
                    throw;

                return null;
            }

            return null;
        }

        internal static int CountNodes(List<TreeNode> listNodes)
        {
            int nCount = 0;

            foreach (TreeNode treeNode in listNodes)
            {
                nCount += CountNodes(treeNode, bNextNode: false);
            }

            return nCount;
        }

        internal static int CountNodes(TreeNode treeNode_in, bool bNextNode = true)
        {
            TreeNode treeNode = treeNode_in;
            int nCount = 0;

            do
            {
                if ((treeNode.Nodes != null) && (treeNode.Nodes.Count > 0))
                {
                    nCount += CountNodes((TreeNode)treeNode.Nodes[0]);
                }

                ++nCount;
            }
            while (bNextNode && ((treeNode = (TreeNode)treeNode.NextNode) != null));

            return nCount;
        }

        internal static string DecodeAttributes(string strAttr)
        {
            FileAttributes nAttr = (FileAttributes)Convert.ToInt32(strAttr, 16);
            string str = "";

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

        static string FormatLine(string strLineType, long nLineNo, string strLine_in = null)
        {
            string strLine_out = strLineType + "\t" + nLineNo;

            if (false == string.IsNullOrWhiteSpace(strLine_in))
            {
                strLine_out += '\t' + strLine_in;
            }

            return strLine_out;
        }

        internal static string FormatSize(string in_str, bool bBytes = false)
        {
            return FormatSize(ulong.Parse(in_str), bBytes);
        }

        internal static string FormatSize(long nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            return FormatSize((ulong)nLength, bBytes, bNoDecimal);
        }

        internal static string FormatSize(ulong nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            double nT = nLength / 1024.0 / 1024.0 / 1024 / 1024 - .05;
            double nG = nLength / 1024.0 / 1024 / 1024 - .05;
            double nM = nLength / 1024.0 / 1024 - .05;
            double nK = nLength / 1024.0 - .05;     // Windows Explorer seems to not round
            string strFmt_big = "###,##0.0";
            string strFormat = bNoDecimal ? "###,###" : strFmt_big;
            string strSz = null;

            if (((int)nT) > 0) strSz = nT.ToString(strFmt_big) + " TB";
            else if (((int)nG) > 0) strSz = nG.ToString(strFmt_big) + " GB";
            else if (((int)nM) > 0) strSz = nM.ToString(strFormat) + " MB";
            else if (((int)nK) > 0) strSz = nK.ToString(strFormat) + " KB";
            else strSz = "1 KB";                    // Windows Explorer mins at 1K

            if (nLength > 0)
            {
                return strSz + (bBytes ? (" (" + nLength.ToString("###,###,###,###,###") + " bytes)") : string.Empty);
            }
            else
            {
                return "0 bytes";
            }
        }

        internal static void SetProperty<T>(object input, T outObj, Expression<Func<T, object>> outExpr)
        {
            if (input == null)
            {
                return;
            }

            ((PropertyInfo)((MemberExpression)outExpr.Body).Member).SetValue(outObj, input, null);
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
            Console.WriteLine(str);
#endif
        }
    }
}
