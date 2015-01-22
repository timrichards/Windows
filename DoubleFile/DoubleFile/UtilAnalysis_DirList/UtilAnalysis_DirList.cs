using System.Windows.Forms;
using System.Threading;         // release mode
using System.Diagnostics;       // debug mode
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

namespace DoubleFile
{
    delegate bool BoolAction();

    class UtilAnalysis_DirList
    {
        internal const string ksHeader01 = "SearchDirLists 0.1";
        internal const string ksStart01 = ksHeader01 + " START";
        internal const string ksEnd01 = ksHeader01 + " END";
        internal const string ksErrorsLoc01 = ksHeader01 + " ERRORS";
        internal const string ksTotalLengthLoc01 = ksHeader01 + " LENGTH";
        internal const string ksDrive01 = ksHeader01 + " DRIVE";
        internal const string ksVolListHeader01 = ksHeader01 + " VOLUME LIST";

        internal const string ksHeader = "SearchDirLists 0.2";
        internal const string ksStart = ksHeader + " START";
        internal const string ksEnd = ksHeader + " END";
        internal const string ksErrorsLoc = ksHeader + " ERRORS";
        internal const string ksTotalLengthLoc = ksHeader + " LENGTH";
        internal const string ksVolume = ksHeader + " VOLUME";
        internal const string ksProjectHeader = ksHeader + " PROJECT";
        internal const string ksCopyScratchpadHeader = ksHeader + " COPYDIRS LIST";
        internal const string ksIgnoreListHeader = ksHeader + " IGNORE LIST";
        internal const string ksUsingFile = "Using file.";
        internal const string ksSaved = "Saved.";
        internal const string ksNotSaved = "Not saved.";
        internal const string ksCantSave = "Can't save. Not mounted.";

        internal const int knColLength = 7;
        internal const int knColLength01 = 5;
        internal const int knColLengthLV = 4;

        internal const string ksLineType_Version = "V";
        internal const string ksLineType_Nickname = "N";
        internal const string ksLineType_Path = "P";
        internal const string ksLineType_DriveInfo = "I";
        internal const string ksLineType_Comment = "C";
        internal const string ksLineType_Start = "S";
        internal const string ksLineType_Directory = "D";
        internal const string ksLineType_File = "F";
        internal const string ksLineType_End = "E";
        internal const string ksLineType_Blank = "B";
        internal const string ksLineType_ErrorDir = "R";
        internal const string ksLineType_ErrorFile = "r";
        internal const string ksLineType_Length = "L";

        internal const string ksFileExt_Listing = "DFL";
        internal const string ksFileExt_Project = "DFP";
        internal const string ksFileExt_Copy = "sdl_copy";
        internal const string ksFileExt_Ignore = "sdl_ignore";

        const int knDriveInfoItems = 11;
        internal static readonly string[] kasDIlabels = new string[knDriveInfoItems]
        {
            "Volume Free",
            "Volume Format",
            "Drive Type",           // DriveInfo
            "Volume Name",
            "Volume Root",
            "Volume Free 2",
            "Volume Size",
            "Volume Label",
            "Drive Model",          // These last three are CIM items
            "Drive Serial",
            "Drive Size"
        };
        internal static readonly bool[] kabDIsizeType = new bool[knDriveInfoItems]
        {
            true, false, false, false, false, true, true, false, false, false, true
        };
        internal static readonly int[] kanDIviewOrder = new int[knDriveInfoItems]
        {
            9, 5, 6, 2, 0, 10, 8, 1, 3, 4, 7
        };
        internal static readonly int[] kanDIoptIfEqTo = new int[knDriveInfoItems]
        {
            -1, -1, -1, 4, -1, 0, -1, -1, -1, -1, -1
        };

        static double static_nLastAssertLoc = -1;
        static DateTime static_dtLastAssert = DateTime.MinValue;

#if (DEBUG == false)
        static bool static_bAssertUp = false;
#endif

        internal static bool Assert(double nLocation, bool bCondition, string strError_in = null, bool bTraceOnly = false)
        {
            if (bCondition) return true;

            if ((static_nLastAssertLoc == nLocation) && ((DateTime.Now - static_dtLastAssert).Seconds < 1))
            {
                return false;
            }

            string strError = "Assertion failed at location " + nLocation + ".";

            if (false == string.IsNullOrWhiteSpace(strError_in))
            {
                strError += "\n\nAdditional information: " + strError_in;
            }

            UtilAnalysis_DirList.WriteLine(strError);
#if (DEBUG)
            Debug.Assert(false, strError);
#else
            if (static_bAssertUp == false)
            {
                bool bTrace = false; // Trace.Listeners.Cast<TraceListener>().Any(i => i is DefaultTraceListener);

                Action messageBox = new Action(() =>
                {
                    MBox.ShowDialog(strError + "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.".PadRight(100), "SearchDirLists Assertion Failure");
                    static_bAssertUp = false;
                });

                if (bTrace)
                {
                    messageBox();
                }
                else if (bTraceOnly == false)
                {
                    static_nLastAssertLoc = nLocation;
                    static_dtLastAssert = DateTime.Now;
                    static_bAssertUp = true;
                    new Thread(new ThreadStart(messageBox)).Start();
                }
            }
#endif
            return false;
        }

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

        internal static void ConvertFile(string strFile)
        {
            string strFile_01 = StrFile_01(strFile);

            if (File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            File.Move(strFile, strFile_01);

            using (StreamWriter file_out = new StreamWriter(strFile))
            {
                using (StreamReader file_in = new StreamReader(strFile_01))
                {
                    string strLine = null;
                    long nLineNo = 0;       // lines number from one
                    bool bAtErrors = false;

                    while ((strLine = file_in.ReadLine()) != null)
                    {
                        ++nLineNo;

                        if (strLine == ksHeader01)
                        {
                            MBox.Assert(1303.4307, nLineNo == 1);
                            file_out.WriteLine(FormatLine(ksLineType_Version, nLineNo, ksHeader));
                            continue;
                        }
                        else if (nLineNo == 2)
                        {
                            file_out.WriteLine(FormatLine(ksLineType_Nickname, nLineNo, strLine));
                            continue;
                        }
                        else if (nLineNo == 3)
                        {
                            file_out.WriteLine(FormatLine(ksLineType_Path, nLineNo, strLine));
                            continue;
                        }
                        else if (strLine == ksDrive01)
                        {
                            MBox.Assert(1303.4308, nLineNo == 4);
                            file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, ksVolume));

                            int ixDriveInfo = 0;
                            for (; ixDriveInfo < kasDIlabels.Length; ++ixDriveInfo)
                            {
                                strLine = file_in.ReadLine();
                                ++nLineNo;

                                if (strLine.Length <= 0)
                                {
                                    break;
                                }

                                file_out.WriteLine(FormatLine(ksLineType_DriveInfo, nLineNo, strLine));
                            }

                            if (ixDriveInfo == kasDIlabels.Length)
                            {
                                strLine = file_in.ReadLine();
                                ++nLineNo;
                            }
                            
                            file_out.WriteLine(FormatLine(ksLineType_Blank, nLineNo));
                            ++nLineNo;
                            strLine = file_in.ReadLine();
                            file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, FormatString(nHeader: 0)));
                            ++nLineNo;
                            strLine = file_in.ReadLine();
                            file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, FormatString(nHeader: 1)));
                            continue;
                        }
                        else if (strLine.Length <= 0)
                        {
                            file_out.WriteLine(FormatLine(ksLineType_Blank, nLineNo));
                            continue;
                        }
                        else if (strLine.StartsWith(ksStart01))
                        {
                            file_out.WriteLine(FormatLine(ksLineType_Start, nLineNo, ksStart));
                            continue;
                        }
                        else if (strLine.StartsWith(ksEnd01))
                        {
                            file_out.WriteLine(FormatLine(ksLineType_End, nLineNo, ksEnd));
                            continue;
                        }
                        else if (strLine == ksErrorsLoc01)
                        {
                            file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, ksErrorsLoc));
                            bAtErrors = true;
                            continue;
                        }
                        else if (strLine.StartsWith(ksTotalLengthLoc01))
                        {
                            string[] arrLine = strLine.Split('\t');

                            file_out.WriteLine(FormatLine(ksLineType_Length, nLineNo, FormatString(strDir: ksTotalLengthLoc, nLength: long.Parse(arrLine[knColLength01]))));
                            continue;
                        }

                        string[] arrLine_A = strLine.Split('\t');
                        string strDir = arrLine_A[0];

                        if (string.IsNullOrWhiteSpace(strDir))
                        {
                            DateTime dtParse;
                            string strTab = null;

                            if ((arrLine_A.Length > 5) && arrLine_A[5].Contains("Trailing whitespace") && DateTime.TryParse(arrLine_A[1], out dtParse))
                            {
                                strTab = "\t";
                            }

                            file_out.WriteLine(FormatLine(bAtErrors ? ksLineType_ErrorFile : ksLineType_File, nLineNo, strTab + strLine));
                            continue;
                        }
                        else if (strDir.Contains(@":\") == false)
                        {
                            MBox.Assert(1303.4311, false);        // all that's left is directories
                            continue;
                        }

                        // directory
                        file_out.WriteLine(FormatLine(bAtErrors ? ksLineType_ErrorDir : ksLineType_Directory, nLineNo, strLine.Replace(@"\\", @"\")));
                    }
                }
            }
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

        internal static string FormatString(string strDir = null, string strFile = null, DateTime? dtCreated = null, DateTime? dtModified = null, string strAttributes = null, long nLength = -1, string strError1 = null, string strError2 = null, int? nHeader = null, string strChecksum = null)
        {
            string strLength = null;
            string strCreated = null;
            string strModified = null;

            if (nLength > -1)
            {
                strLength = nLength.ToString();
            }

            if (dtCreated != null)
            {
                strCreated = dtCreated.ToString();
            }

            if (dtModified != null)
            {
                strModified = dtModified.ToString();
            }

            if (string.IsNullOrWhiteSpace(strDir + strFile + strCreated + strModified + strAttributes + strLength + strError1 + strError2 + strChecksum))
            {
                MBox.Assert(1303.4314, nHeader is int);

                if (nHeader == 0)
                {
                    return "2" + '\t' + "3" + '\t' + "4" + '\t' + "5" + '\t' + "6" + '\t' + "7" + '\t' + "8" + '\t' + "9" + '\t' + "10";
                }
                else if (nHeader == 1)
                {
                    return "Dir" + '\t' + "File" + '\t' + "Created" + '\t' + "Modded" + '\t' + "Attrib" + '\t' + "Length" + '\t' + "Error1" + '\t' + "Error2" + '\t' + "FakeChecksum";
                }
            }

            bool bDbgCheck = false;

            if (((strDir ?? "").TrimEnd() != (strDir ?? "")) || ((strFile ?? "").TrimEnd() != (strFile ?? "")))
            {
                strError1 += " Trailing whitespace";
                strError1.Trim();
                MBox.Assert(1303.4315, (false == string.IsNullOrWhiteSpace(strDir)) || (false == string.IsNullOrWhiteSpace(strFile)));
                bDbgCheck = true;
            }

            string strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2 + '\t' + strChecksum).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                string[] strArray = strRet.Split('\t');
                DateTime dtParse = DateTime.MinValue;

                if (strArray[knColLength01].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    MBox.Assert(1303.4316, false);
                }
#endif
            }

            return strRet;
        }

        protected static string StrFile_01(string strFile)
        {
            return Path.Combine(Path.GetDirectoryName(strFile),
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));
        }

        internal static void SetProperty<T>(object input, T outObj, Expression<Func<T, object>> outExpr)
        {
            if (input == null)
            {
                return;
            }

            ((PropertyInfo)((MemberExpression)outExpr.Body).Member).SetValue(outObj, input, null);
        }

        internal static bool ValidateFile(string strSaveAs)
        {
            if (File.Exists(strSaveAs) == false) return false;

            string[] arrLine = File.ReadLines(strSaveAs).Take(1).ToArray();

            if (arrLine.Length <= 0) return false;

            bool bConvertFile = false;

            if (arrLine[0] == ksHeader01)
            {
                UtilAnalysis_DirList.WriteLine("Converting " + strSaveAs);
                ConvertFile(strSaveAs);
                UtilAnalysis_DirList.WriteLine("File converted to " + ksHeader);
                bConvertFile = true;
            }

            string[] arrToken = File.ReadLines(strSaveAs).Take(1).ToArray()[0].Split('\t');

            if (arrToken.Length < 3) return false;
            if (arrToken[2] != ksHeader) return false;

            string[] arrLine_A = File.ReadLines(strSaveAs).Where(s => s.StartsWith(ksLineType_Length)).ToArray();

            if (arrLine_A.Length <= 0) return false;

            string[] arrToken_A = arrLine_A[0].Split('\t');

            if (arrToken_A.Length < 3) return false;
            if (arrToken_A[2] != ksTotalLengthLoc) return false;

            string strFile_01 = StrFile_01(strSaveAs);

            if (bConvertFile && File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            return true;
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
