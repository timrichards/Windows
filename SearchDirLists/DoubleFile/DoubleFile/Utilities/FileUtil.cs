using System;
using System.IO;

namespace DoubleFile
{
    class FileUtil
    {
        internal static string CheckNTFS_chars(ref string strFile, bool bFile = false)
        {
            char[] arrChar = bFile ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            int nIx = -1;

            if ((nIx = strFile.IndexOfAny(arrChar)) > -1)
            {
                string strRet = "NTFS ASCII " + ((int)strFile[nIx]).ToString();

                strFile = strFile.Replace("\n", "").Replace("\r", "").Replace("\t", "");    // program-incompatible
                return strRet;
            }
            else
            {
                return null;
            }
        }

        internal static bool FormatPath(ref string strPath, bool bFailOnDirectory = true)
        {
            while (strPath.Contains(@"\\"))
            {
                strPath = strPath.Replace(@"\\", @"\");
            }

            string strDirName = Path.GetDirectoryName(strPath);

            if (string.IsNullOrWhiteSpace(strDirName) || Directory.Exists(strDirName))
            {
                string strCapDrive = strPath.Substring(0, strPath.IndexOf(@":\") + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath == strCapDrive.ToUpper())
                {
                    MBox.Assert(1303.4312, strDirName == null);
                }
                else
                {
                    strPath = strPath.TrimEnd('\\');
                    MBox.Assert(1303.4313, false == string.IsNullOrWhiteSpace(strDirName));
                }
            }
            else if (bFailOnDirectory)
            {
                return false;
            }

            return true;
        }

        internal static bool FormatPath(ref string strPath, ref string strSaveAs, bool bFailOnDirectory = true)
        {
            if (false == string.IsNullOrWhiteSpace(strPath))
            {
                strPath += '\\';

                if (FormatPath(ref strPath, bFailOnDirectory) == false)
                {
                    MBox.ShowDialog("Error in Source path.", "Save Directory Listing");
                    return false;
                }
            }

            if (false == string.IsNullOrWhiteSpace(strSaveAs))
            {
                strSaveAs = Path.GetFullPath(strSaveAs.Trim());

                if (FormatPath(ref strSaveAs, bFailOnDirectory) == false)
                {
                    MBox.ShowDialog("Error in Save filename.", "Save Directory Listing");
                    return false;
                }
            }

            return true;
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
    }
}
