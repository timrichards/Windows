using System;
using System.IO;
using System.Linq;

namespace DoubleFile
{
    class FileParse
    {
        internal const string mSTRheader01 = "SearchDirLists 0.1";
        internal const string mSTRstart01 = mSTRheader01 + " START";
        internal const string mSTRend01 = mSTRheader01 + " END";
        internal const string mSTRerrorsLoc01 = mSTRheader01 + " ERRORS";
        internal const string mSTRtotalLengthLoc01 = mSTRheader01 + " LENGTH";
        internal const string mSTRdrive01 = mSTRheader01 + " DRIVE";
        internal const string mSTRvolListHeader01 = mSTRheader01 + " VOLUME LIST";

        internal const string mSTRheader = "SearchDirLists 0.2";
        internal const string mSTRstart = mSTRheader + " START";
        internal const string mSTRend = mSTRheader + " END";
        internal const string mSTRerrorsLoc = mSTRheader + " ERRORS";
        internal const string mSTRtotalLengthLoc = mSTRheader + " LENGTH";
        internal const string mSTRdrive = mSTRheader + " DRIVE";
        internal const string mSTRvolListHeader = mSTRheader + " VOLUME LIST";
        internal const string mSTRcopyScratchpadHeader = mSTRheader + " COPYDIRS LIST";
        internal const string mSTRignoreListHeader = mSTRheader + " IGNORE LIST";
        internal const string mSTRusingFile = "Using file.";
        internal const string mSTRsaved = "Saved.";
        internal const string mSTRnotSaved = "Not saved.";
        internal const string mSTRcantSave = "Can't save. Not mounted.";

        internal const int mNcolLength = 7;
        internal const int mNcolLength01 = 5;
        internal const int mNcolLengthLV = 4;

        internal const string mSTRlineType_Version = "V";
        internal const string mSTRlineType_Nickname = "N";
        internal const string mSTRlineType_Path = "P";
        internal const string mSTRlineType_DriveInfo = "I";
        internal const string mSTRlineType_Comment = "C";
        internal const string mSTRlineType_Start = "S";
        internal const string mSTRlineType_Directory = "D";
        internal const string mSTRlineType_File = "F";
        internal const string mSTRlineType_End = "E";
        internal const string mSTRlineType_Blank = "B";
        internal const string mSTRlineType_ErrorDir = "R";
        internal const string mSTRlineType_ErrorFile = "r";
        internal const string mSTRlineType_Length = "L";

        internal const string mSTRfileExt_Listing = "sdl_list";
        internal const string mSTRfileExt_Volume = "sdl_vol";
        internal const string mSTRfileExt_Copy = "sdl_copy";
        internal const string mSTRfileExt_Ignore = "sdl_ignore";

        const int knDriveInfoItems = 11;
        internal static readonly string[] mAstrDIlabels = new string[knDriveInfoItems]
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
        internal static readonly bool[] mAbDIsizeType = new bool[knDriveInfoItems]
        {
            true, false, false, false, false, true, true, false, false, false, true
        };
        internal static readonly int[] mAnDIviewOrder = new int[knDriveInfoItems]
        {
            9, 5, 6, 2, 0, 10, 8, 1, 3, 4, 7
        };
        internal static readonly int[] mAnDIoptIfEqTo = new int[knDriveInfoItems]
        {
            -1, -1, -1, 4, -1, 0, -1, -1, -1, -1, -1
        };

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

                        if (strLine == mSTRheader01)
                        {
                            System.Diagnostics.Debug.Assert(nLineNo == 1);
                            file_out.WriteLine(FormatLine(mSTRlineType_Version, nLineNo, mSTRheader));
                            continue;
                        }
                        else if (nLineNo == 2)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Nickname, nLineNo, strLine));
                            continue;
                        }
                        else if (nLineNo == 3)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Path, nLineNo, strLine));
                            continue;
                        }
                        else if (strLine == mSTRdrive01)
                        {
                            System.Diagnostics.Debug.Assert(nLineNo == 4);
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, mSTRdrive));

                            int ixDriveInfo = 0;
                            for (; ixDriveInfo < mAstrDIlabels.Length; ++ixDriveInfo)
                            {
                                strLine = file_in.ReadLine();
                                ++nLineNo;

                                if (strLine.Length <= 0)
                                {
                                    break;
                                }

                                file_out.WriteLine(FormatLine(mSTRlineType_DriveInfo, nLineNo, strLine));
                            }

                            if (ixDriveInfo == mAstrDIlabels.Length)
                            {
                                strLine = file_in.ReadLine();
                                ++nLineNo;
                            }

                            file_out.WriteLine(FormatLine(mSTRlineType_Blank, nLineNo));
                            ++nLineNo;
                            strLine = file_in.ReadLine();
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, FormatString(nHeader: 0)));
                            ++nLineNo;
                            strLine = file_in.ReadLine();
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, FormatString(nHeader: 1)));
                            continue;
                        }
                        else if (strLine.Length <= 0)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Blank, nLineNo));
                            continue;
                        }
                        else if (strLine.StartsWith(mSTRstart01))
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Start, nLineNo, mSTRstart));
                            continue;
                        }
                        else if (strLine.StartsWith(mSTRend01))
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_End, nLineNo, mSTRend));
                            continue;
                        }
                        else if (strLine == mSTRerrorsLoc01)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, mSTRerrorsLoc));
                            bAtErrors = true;
                            continue;
                        }
                        else if (strLine.StartsWith(mSTRtotalLengthLoc01))
                        {
                            string[] arrLine = strLine.Split('\t');

                            file_out.WriteLine(FormatLine(mSTRlineType_Length, nLineNo, FormatString(strDir: mSTRtotalLengthLoc, nLength: long.Parse(arrLine[mNcolLength01]))));
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

                            file_out.WriteLine(FormatLine(bAtErrors ? mSTRlineType_ErrorFile : mSTRlineType_File, nLineNo, strTab + strLine));
                            continue;
                        }
                        else if (strDir.Contains(@":\") == false)
                        {
                            System.Diagnostics.Debug.Assert(false);        // all that's left is directories
                            continue;
                        }

                        // directory
                        file_out.WriteLine(FormatLine(bAtErrors ? mSTRlineType_ErrorDir : mSTRlineType_Directory, nLineNo, strLine.Replace(@"\\", @"\")));
                    }
                }
            }
        }

        static string FormatLine(string strLineType, long nLineNo, string strLine_in = null)
        {
            string strLine_out = strLineType + "\t" + nLineNo;

            if (string.IsNullOrWhiteSpace(strLine_in) == false)
            {
                strLine_out += '\t' + strLine_in;
            }

            return strLine_out;
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

            if (false == string.IsNullOrWhiteSpace(strDir + strFile + strCreated + strModified + strAttributes + strLength + strError1 + strError2 + strChecksum) == false)
            {
                System.Diagnostics.Debug.Assert(nHeader is int);

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
                System.Diagnostics.Debug.Assert(false == string.IsNullOrWhiteSpace(strDir) || false == string.IsNullOrWhiteSpace(strFile));
                bDbgCheck = true;
            }

            string strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2 + '\t' + strChecksum).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                string[] strArray = strRet.Split('\t');
                DateTime dtParse = DateTime.MinValue;

                if (strArray[mNcolLength01].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    System.Diagnostics.Debug.Assert(false);
                }
#endif
            }

            return strRet;
        }

        internal static bool ReadHeader(string strFile, out string strVolumeName, out string strPath)
        {
            strVolumeName = null;
            strPath = null;

            if (ValidateFile(strFile) == false)
            {
                return false;
            }

            using (var sr = new System.IO.StreamReader(strFile))
            {
                string line = null;

                if ((line = sr.ReadLine()) == null) return false;
                if ((line = sr.ReadLine()) == null) return false;
                if (line.StartsWith(mSTRlineType_Nickname) == false) return false;

                string[] arrLine = line.Split('\t');
                strVolumeName = string.Empty;

                if (arrLine.Length > 2) strVolumeName = arrLine[2];
                if ((line = sr.ReadLine()) == null) return false;
                if (line.StartsWith(mSTRlineType_Path) == false) return false;
                arrLine = line.Split('\t');
                if (arrLine.Length < 3) return false;
                strPath = arrLine[2];
            }

            return true;
        }

        protected static string StrFile_01(string strFile)
        {
            return Path.Combine(Path.GetDirectoryName(strFile),
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));
        }

        internal static bool ValidateFile(string strFile)
        {
            if (File.Exists(strFile) == false) return false;

            string[] arrLine = File.ReadLines(strFile).Take(1).ToArray();

            if (arrLine.Length <= 0) return false;

            bool bConvertFile = false;

            if (arrLine[0] == mSTRheader01)
            {
                Utilities.WriteLine("Converting " + strFile);
                ConvertFile(strFile);
                Utilities.WriteLine("File converted to " + mSTRheader);
                bConvertFile = true;
            }

            string[] arrToken = File.ReadLines(strFile).Take(1).ToArray()[0].Split('\t');

            if (arrToken.Length < 3) return false;
            if (arrToken[2] != mSTRheader) return false;

            string[] arrLine_A = File.ReadLines(strFile).Where(s => s.StartsWith(mSTRlineType_Length)).ToArray();

            if (arrLine_A.Length <= 0) return false;

            string[] arrToken_A = arrLine_A[0].Split('\t');

            if (arrToken_A.Length < 3) return false;
            if (arrToken_A[2] != mSTRtotalLengthLoc) return false;

            string strFile_01 = StrFile_01(strFile);

            if (bConvertFile && File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            return true;
        }
    }
}
