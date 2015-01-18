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
        internal const string mSTRvolume = mSTRheader + " VOLUME";
        internal const string mSTRvolListHeader = mSTRheader + " VOLUME LIST";
        internal const string mSTRcopyScratchpadHeader = mSTRheader + " COPYDIRS LIST";
        internal const string mSTRignoreListHeader = mSTRheader + " IGNORE LIST";
        internal const string mSTRusingFile = "Using file.";
        internal const string mSTRsaved = "Saved.";
        internal const string mSTRnotSaved = "Not saved.";
        internal const string mSTRcantSave = "Can't save. Not mounted.";
        internal const string mSTRinclude = "Yes";

        internal const int mNcolLength = 7;
        internal const int mNcolLength01 = 5;
        internal const int mNcolLengthLV = 4;

        internal const string mSTRlineType_Version = "V";
        internal const string mSTRlineType_Nickname = "N";
        internal const string mSTRlineType_Path = "P";
        internal const string mSTRlineType_VolumeInfo = "I";
        internal const string mSTRlineType_VolumeInfo_Free = "Info: Free";
        internal const string mSTRlineType_VolumeInfo_Format = "Info: Format";
        internal const string mSTRlineType_VolumeInfo_DriveType = "Info: DriveType";
        internal const string mSTRlineType_VolumeInfo_Name = "Info: Name";
        internal const string mSTRlineType_VolumeInfo_Root = "Info: Root";
        internal const string mSTRlineType_VolumeInfo_Free2 = "Info: Free2";
        internal const string mSTRlineType_VolumeInfo_Size = "Info: Size";
        internal const string mSTRlineType_VolumeInfo_Label = "Info: Label";
        internal const string mSTRlineType_VolumeInfo_DriveModel = "Info: DriveModel";
        internal const string mSTRlineType_VolumeInfo_DriveSerial = "Info: DriveSerial";
        internal const string mSTRlineType_VolumeInfo_DriveSize = "Info: DriveSize";
        internal const string mSTRlineType_Comment = "C";
        internal const string mSTRlineType_Start = "S";
        internal const string mSTRlineType_Directory = "D";
        internal const string mSTRlineType_File = "F";
        internal const string mSTRlineType_End = "E";
        internal const string mSTRlineType_Blank = "B";
        internal const string mSTRlineType_ErrorDir = "R";
        internal const string mSTRlineType_ErrorFile = "r";
        internal const string mSTRlineType_Length = "L";

        internal const string mSTRfileExt_Listing = "DFL";
        internal const string mSTRfileExt_Project = "DFP";
        internal const string mSTRfileExt_Copy = "sdl_copy";
        internal const string mSTRfileExt_Ignore = "sdl_ignore";

        internal const int knDriveInfoItems = 11;
        internal const int knDriveModel = 12;           // backwards compatibility when Info: isn't in the header
        internal const int knDriveSerial = 13;          // ...line numbering starts at one in the file and zero in the program

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
                            MBox.Assert(0, nLineNo == 1);
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
                            MBox.Assert(0, nLineNo == 4);
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, mSTRvolume));

                            string[] astrInfoLineTypes = new string[knDriveInfoItems]
                            {
                                mSTRlineType_VolumeInfo_Free, mSTRlineType_VolumeInfo_Format, mSTRlineType_VolumeInfo_DriveType,
                                mSTRlineType_VolumeInfo_Name, mSTRlineType_VolumeInfo_Root, mSTRlineType_VolumeInfo_Free2,
                                mSTRlineType_VolumeInfo_Size, mSTRlineType_VolumeInfo_Label,
                                mSTRlineType_VolumeInfo_DriveModel, mSTRlineType_VolumeInfo_DriveSerial,
                                mSTRlineType_VolumeInfo_DriveSize
                            };

                            var bHitBlankLine = false;
                            
                            for (var ixDriveInfo = 0; ixDriveInfo < knDriveInfoItems; ++ixDriveInfo)
                            {
                                strLine = bHitBlankLine ? "" : file_in.ReadLine();

                                if ((false == bHitBlankLine) && (strLine.Length <= 0))
                                {
                                    bHitBlankLine = true;
                                }

                                ++nLineNo;
                                file_out.WriteLine(FormatLine(astrInfoLineTypes[ixDriveInfo], nLineNo, strLine));
                            }

                            if (false == bHitBlankLine)
                            {
                                strLine = file_in.ReadLine();
                            }
                            
                            ++nLineNo;
                            file_out.WriteLine(FormatLine(mSTRlineType_Blank, nLineNo));

                            strLine = file_in.ReadLine();
                            ++nLineNo;
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, FormatString(nHeader: 0)));

                            strLine = file_in.ReadLine();
                            ++nLineNo;
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
                            MBox.Assert(0, false);        // all that's left is directories
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
            return strLineType + "\t" + nLineNo + '\t' + (strLine_in ?? "").Trim();
        }

        internal static string FormatString(string strDir = null, string strFile = null, DateTime? dtCreated = null, DateTime? dtModified = null, string strAttributes = null, long nLength = -1, string strError1 = null, string strError2 = null, int? nHeader = null, string strHash = null)
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

            if (false == string.IsNullOrWhiteSpace(strDir + strFile + strCreated + strModified + strAttributes + strLength + strError1 + strError2 + strHash) == false)
            {
                MBox.Assert(0, nHeader is int);

                if (nHeader == 0)
                {
                    return "2" + '\t' + "3" + '\t' + "4" + '\t' + "5" + '\t' + "6" + '\t' + "7" + '\t' + "8" + '\t' + "9" + '\t' + "10";
                }
                else if (nHeader == 1)
                {
                    return "Dir" + '\t' + "File" + '\t' + "Created" + '\t' + "Modded" + '\t' + "Attrib" + '\t' + "Length" + '\t' + "Error1" + '\t' + "Error2" + '\t' + "Hash";
                }
            }

            bool bDbgCheck = false;

            if (((strDir ?? "").TrimEnd() != (strDir ?? "")) || ((strFile ?? "").TrimEnd() != (strFile ?? "")))
            {
                strError1 += " Trailing whitespace";
                strError1.Trim();
                MBox.Assert(0, false == string.IsNullOrWhiteSpace(strDir) || false == string.IsNullOrWhiteSpace(strFile));
                bDbgCheck = true;
            }

            string strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2 + '\t' + strHash).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                string[] strArray = strRet.Split('\t');
                DateTime dtParse = DateTime.MinValue;

                if (strArray[mNcolLength01].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    MBox.Assert(0, false);
                }
#endif
            }

            return strRet;
        }

        internal static bool ReadHeader(string strFile, out LVitem_VolumeVM lvItem_out)
        {
            lvItem_out = null;

            if (ValidateFile(strFile) == false)
            {
                return false;
            }

            LVitem_VolumeVM lvItem = new LVitem_VolumeVM();

            lvItem.ListingFile = strFile;
            lvItem.Status = mSTRusingFile;
            lvItem.Include = true;

            using (var sr = new System.IO.StreamReader(strFile))
            {
                string line = null;

                if ((line = sr.ReadLine()) == null) return false;
                if ((line = sr.ReadLine()) == null) return false;
                if (line.StartsWith(mSTRlineType_Nickname) == false) return false;

                string[] arrLine = line.Split('\t');

                if (arrLine.Length > 2) lvItem.Nickname = arrLine[2];
                if ((line = sr.ReadLine()) == null) return false;
                if (line.StartsWith(mSTRlineType_Path) == false) return false;

                // unkosher lambda "byref parameters"
                bool bReadAttributeReturnValue = false;
                string strReadAttributeReturnValue = null;
                Action<string> ReadAttribute = new Action<string>(s =>
                {
                    bReadAttributeReturnValue = false;
                    strReadAttributeReturnValue = null;

                    var astr = s.Split('\t');

                    MBox.Assert(0, astr.Length == 3);

                    if (astr.Length < 3)
                    {
                        return; // from lambda
                    }

                    strReadAttributeReturnValue = astr[2];
                    bReadAttributeReturnValue = true;
                });

                ReadAttribute(line);
                if (bReadAttributeReturnValue == false) return false;
                lvItem.SourcePath = strReadAttributeReturnValue;

                File.ReadLines(strFile).Skip(knDriveModel).Take(1)
                //  .Where(s => s.StartsWith(mSTRlineType_VolumeInfo_DriveModel))
                    .FirstOnlyAssert(s =>
                {
                    if (s.StartsWith(mSTRlineType_VolumeInfo))
                    {
                        ReadAttribute(s);
                        if (bReadAttributeReturnValue) lvItem.DriveModel = strReadAttributeReturnValue;
                    }
                    else
                    {
                        MBox.Assert(0, false);
                    }
                });

                File.ReadLines(strFile).Skip(knDriveSerial).Take(1)
                //   .Where(s => s.StartsWith(mSTRlineType_VolumeInfo_DriveSerial))
                    .FirstOnlyAssert(s =>
                {
                    if (s.StartsWith(mSTRlineType_VolumeInfo))
                    {
                        ReadAttribute(s);
                        if (bReadAttributeReturnValue) lvItem.DriveSerial = strReadAttributeReturnValue;
                    }
                    else
                    {
                        MBox.Assert(0, false);
                    }
                });
            }

            lvItem_out = lvItem;
            return true;
        }

        internal static string StrFile_01(string strFile)
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
