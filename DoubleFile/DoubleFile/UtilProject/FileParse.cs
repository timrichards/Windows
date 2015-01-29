using System;
using System.IO;
using System.Linq;

namespace DoubleFile
{
    class FileParse
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
        internal const string ksError = "Error during save.";
        internal const string ksInclude = "Yes";

        internal const int knColLength = 7;
        internal const int knColLength01 = 5;
        internal const int knColLengthLV = 4;

        internal const string ksLineType_Version = "V";
        internal const string ksLineType_Nickname = "N";
        internal const string ksLineType_Path = "P";
        internal const string ksLineType_VolumeInfo = "I";
        internal const string ksLineType_VolumeInfo_Free = "Info: Free";
        internal const string ksLineType_VolumeInfo_Format = "Info: Format";
        internal const string ksLineType_VolumeInfo_DriveType = "Info: DriveType";
        internal const string ksLineType_VolumeInfo_Name = "Info: Name";
        internal const string ksLineType_VolumeInfo_Root = "Info: Root";
        internal const string ksLineType_VolumeInfo_Free2 = "Info: Free2";
        internal const string ksLineType_VolumeInfo_Size = "Info: Size";
        internal const string ksLineType_VolumeInfo_Label = "Info: Label";
        internal const string ksLineType_VolumeInfo_DriveModel = "Info: DriveModel";
        internal const string ksLineType_VolumeInfo_DriveSerial = "Info: DriveSerial";
        internal const string ksLineType_VolumeInfo_DriveSize = "Info: DriveSize";
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

        internal const int knDriveInfoItems = 11;

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
                            MBox.Assert(0, nLineNo == 1);
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
                            MBox.Assert(0, nLineNo == 4);
                            file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, ksVolume));

                            string[] astrInfoLineTypes = new string[knDriveInfoItems]
                            {
                                ksLineType_VolumeInfo_Free, ksLineType_VolumeInfo_Format, ksLineType_VolumeInfo_DriveType,
                                ksLineType_VolumeInfo_Name, ksLineType_VolumeInfo_Root, ksLineType_VolumeInfo_Free2,
                                ksLineType_VolumeInfo_Size, ksLineType_VolumeInfo_Label,
                                ksLineType_VolumeInfo_DriveModel, ksLineType_VolumeInfo_DriveSerial,
                                ksLineType_VolumeInfo_DriveSize
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
                            file_out.WriteLine(FormatLine(ksLineType_Blank, nLineNo));

                            strLine = file_in.ReadLine();
                            ++nLineNo;
                            file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, FormatString(nHeader: 0)));

                            strLine = file_in.ReadLine();
                            ++nLineNo;
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
                            file_out.WriteLine(FormatLine(ksLineType_Start, nLineNo, ksStart + strLine.Replace(ksStart01, "")));
                            continue;
                        }
                        else if (strLine.StartsWith(ksEnd01))
                        {
                            file_out.WriteLine(FormatLine(ksLineType_End, nLineNo, ksEnd + strLine.Replace(ksEnd01, "")));
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
                            MBox.Assert(0, false);        // all that's left is directories
                            continue;
                        }

                        // directory
                        file_out.WriteLine(FormatLine(bAtErrors ? ksLineType_ErrorDir : ksLineType_Directory, nLineNo, strLine.Replace(@"\\", @"\")));
                    }
                }
            }
        }

        static string FormatLine(string strLineType, long nLineNo, string strLine_in = null)
        {
            return strLineType + "\t" + nLineNo + '\t' + (strLine_in ?? "").TrimEnd();
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

                if (strArray[knColLength01].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    MBox.Assert(0, false);
                }
#endif
            }

            return strRet;
        }

        internal static bool ReadHeader(string strFile, out LVitem_ProjectVM lvItem_out)
        {
            lvItem_out = null;

            if (ValidateFile(strFile) == false)
            {
                return false;
            }

            LVitem_ProjectVM lvItem = new LVitem_ProjectVM();

            lvItem.ListingFile = strFile;
            lvItem.Status = ksUsingFile;
            lvItem.Include = true;

            using (var sr = new System.IO.StreamReader(strFile))
            {
                string line = null;

                if ((line = sr.ReadLine()) == null)
                    return false;

                if ((line = sr.ReadLine()) == null)
                    return false;

                if (line.StartsWith(ksLineType_Nickname) == false)
                    return false;

                string[] arrLine = line.Split('\t');

                if (arrLine.Length > 2)
                    lvItem.Nickname = arrLine[2];

                if ((line = sr.ReadLine()) == null)
                    return false;

                if (line.StartsWith(ksLineType_Path) == false)
                    return false;

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

                if (bReadAttributeReturnValue == false)
                    return false;

                lvItem.SourcePath = strReadAttributeReturnValue;

                File.ReadLines(strFile).Skip(4).Take(knDriveInfoItems)
                    .Where(s => s.StartsWith(ksLineType_VolumeInfo_DriveModel))
                    .FirstOnlyAssert(s =>
                {
                    ReadAttribute(s);

                    if (bReadAttributeReturnValue)
                        lvItem.DriveModel = strReadAttributeReturnValue;
                });

                File.ReadLines(strFile).Skip(4).Take(knDriveInfoItems)
                    .Where(s => s.StartsWith(ksLineType_VolumeInfo_DriveSerial))
                    .FirstOnlyAssert(s =>
                {
                    ReadAttribute(s);

                    if (bReadAttributeReturnValue)
                        lvItem.DriveSerial = strReadAttributeReturnValue;
                });

                try
                {
                    lvItem.ScannedLength = ulong.Parse(     // redundant parse confirms it's a number
                        File.ReadLines(strFile).Where(s => s.StartsWith(ksLineType_Length)).ToArray()[0]
                        .Split('\t')[knColLength]).ToString();
                }
                catch
                {
                    return false;
                }
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

            if (arrLine[0] == ksHeader01)
            {
                UtilProject.WriteLine("Converting " + strFile);
                ConvertFile(strFile);
                UtilProject.WriteLine("File converted to " + ksHeader);
                bConvertFile = true;
            }

            string[] arrToken = File.ReadLines(strFile).Take(1).ToArray()[0].Split('\t');

            if (arrToken.Length < 3) return false;
            if (arrToken[2] != ksHeader) return false;

            string[] arrLine_A = File.ReadLines(strFile).Where(s => s.StartsWith(ksLineType_Length)).ToArray();

            if (arrLine_A.Length <= 0) return false;

            string[] arrToken_A = arrLine_A[0].Split('\t');

            if (arrToken_A.Length < 3) return false;
            if (arrToken_A[2] != ksTotalLengthLoc) return false;

            string strFile_01 = StrFile_01(strFile);

            if (bConvertFile && File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            return true;
        }
    }
}
