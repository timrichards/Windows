using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

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

        internal const string ksHeader = "SearchDirLists 0.2A";
        internal const string ksHeader02 = "SearchDirLists 0.2";
        internal const string ksStart = ksHeader02 + " START";
        internal const string ksEnd = ksHeader02 + " END";
        internal const string ksErrorsLoc = ksHeader02 + " ERRORS";
        internal const string ksTotalLengthLoc = ksHeader02 + " LENGTH";
        internal const string ksVolume = ksHeader02 + " VOLUME";
        internal const string ksProjectHeader = ksHeader02 + " PROJECT";
        internal const string ksCopyScratchpadHeader = ksHeader02 + " COPYDIRS LIST";
        internal const string ksIgnoreListHeader = ksHeader02 + " IGNORE LIST";
        static internal readonly string ksUsingFile = Util.Localized("Status_UsingFile");
        internal const string ksSaved = "Saved.";
        static internal readonly string ksNotSaved = Util.Localized("Status_NotSaved");
        internal const string ksError = "Error during save.";
        static internal readonly string ksIncludeYes = Util.Localized("Include_Yes");
        static internal readonly string ksIncludeNo = Util.Localized("Include_No");

        internal const int knColLength = 7;
        internal const int knColLength01 = 5;
        internal const int knColLengthLV = knColLength - 3;

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

        static internal readonly IReadOnlyList<string>
            kasDIlabels = new string[knDriveInfoItems]
        {
            "Volume Free",
            "Volume Format",
            "Drive Type",           // DriveInfo
            "Volume Name",
            "Volume Root",
            "Volume Free 2",
            "Volume Size",
            "Volume Label",
            "Drive Model #",        // These last three are CIM items
            "Drive Serial #",
            "Drive Size"
        };
        static internal readonly IReadOnlyList<bool>
            kabDIsizeType = new bool[knDriveInfoItems]
            { true, false, false, false, false, true, true, false, false, false, true };
        static internal readonly IReadOnlyList<int>
            kanDIviewOrder = new int[knDriveInfoItems] { 9, 5, 6, 2, 0, 10, 8, 1, 3, 4, 7 };
        static internal readonly IReadOnlyList<int>
            kanDIoptIfEqTo = new int[knDriveInfoItems] { -1, -1, -1, 4, -1, 0, -1, -1, -1, -1, -1 };

        static internal string CheckNTFS_chars(ref string strFile, bool bFile = false)
        {
            var arrChar = bFile ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            var nIx = strFile.IndexOfAny(arrChar);

            if (0 > nIx)
                return null;

            int badChar = strFile[nIx];

            strFile = strFile.Replace("\n", "").Replace("\r", "").Replace("\t", "");    // program-incompatible
            return "NTFS ASCII " + badChar;
        }

        static void ConvertFile(string strFile)
        {
            var strFile_01 = StrFile_01(strFile);

            if (LocalIsoStore.FileExists(strFile_01))
                LocalIsoStore.DeleteFile(strFile_01);

            strFile.FileMoveToIso(strFile_01);

            string strLine = null;
            var nLineNo = 0;       // lines number from one
            var bAtErrors = false;

            using (var file_out = new StreamWriter(LocalIsoStore.CreateFile(strFile)))
            using (var file_in = new StreamReader(LocalIsoStore.OpenFile(strFile_01, FileMode.Open)))
            while (null !=
                (strLine = file_in.ReadLine()))
            {
                ++nLineNo;

                switch (nLineNo)
                {
                    case 1:
                    {
                        Util.Assert(99952, ksHeader01 == strLine);
                        file_out.WriteLine(FormatLine(ksLineType_Version, nLineNo, ksHeader));
                        continue;
                    }

                    case 2:
                    {
                        file_out.WriteLine(FormatLine(ksLineType_Nickname, nLineNo, strLine));
                        continue;
                    }

                    case 3:
                    {
                        file_out.WriteLine(FormatLine(ksLineType_Path, nLineNo, strLine));
                        continue;
                    }

                    case 4:
                    {
                        Util.Assert(99951, ksDrive01 == strLine);
                        file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, ksVolume));

                        var astrInfoLineTypes = new string[knDriveInfoItems]
                        {
                            ksLineType_VolumeInfo_Free, ksLineType_VolumeInfo_Format, ksLineType_VolumeInfo_DriveType,
                            ksLineType_VolumeInfo_Name, ksLineType_VolumeInfo_Root, ksLineType_VolumeInfo_Free2,
                            ksLineType_VolumeInfo_Size, ksLineType_VolumeInfo_Label,
                            ksLineType_VolumeInfo_DriveModel, ksLineType_VolumeInfo_DriveSerial,
                            ksLineType_VolumeInfo_DriveSize
                        };

                        var bHitBlankLine = false;

                        for (var ixDriveInfo = 0; knDriveInfoItems > ixDriveInfo; ++ixDriveInfo)
                        {
                            strLine = bHitBlankLine ? "" : file_in.ReadLine();

                            if (null == strLine)
                            {
                                Util.Assert(99950, false);
                                return;
                            }

                            if ((false == bHitBlankLine) &&
                                (0 == strLine.Length))
                            {
                                bHitBlankLine = true;
                            }

                            ++nLineNo;
                            file_out.WriteLine(FormatLine(astrInfoLineTypes[ixDriveInfo], nLineNo, strLine));
                        }

                        if (false == bHitBlankLine)
                            file_in.ReadLine();

                        ++nLineNo;
                        file_out.WriteLine(FormatLine(ksLineType_Blank, nLineNo));

                        file_in.ReadLine();
                        ++nLineNo;
                        file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, FormatString(nHeader: 0)));

                        file_in.ReadLine();
                        ++nLineNo;
                        file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, FormatString(nHeader: 1)));
                        continue;
                    }

                    default:
                    {
                        break;
                    }
                }

                if (0 == strLine.Length)
                {
                    file_out.WriteLine(FormatLine(ksLineType_Blank, nLineNo));
                    continue;
                }
                
                if (strLine.StartsWith(ksStart01))
                {
                    file_out.WriteLine(FormatLine(ksLineType_Start, nLineNo, ksStart + strLine.Replace(ksStart01, "")));
                    continue;
                }
                
                if (strLine.StartsWith(ksEnd01))
                {
                    file_out.WriteLine(FormatLine(ksLineType_End, nLineNo, ksEnd + strLine.Replace(ksEnd01, "")));
                    continue;
                }
                
                if (ksErrorsLoc01 == strLine)
                {
                    file_out.WriteLine(FormatLine(ksLineType_Comment, nLineNo, ksErrorsLoc));
                    bAtErrors = true;
                    continue;
                }
                
                if (strLine.StartsWith(ksTotalLengthLoc01))
                {
                    file_out.WriteLine(FormatLine(ksLineType_Length, nLineNo,
                        FormatString(strDir: ksTotalLengthLoc,
                        nLength: ("" + strLine.Split('\t')[knColLength01]).ToUlong())));

                    continue;
                }

                var arrLine_A = strLine.Split('\t');
                var strDir = arrLine_A[0];

                if (string.IsNullOrWhiteSpace(strDir))
                {
                    DateTime dtParse;
                    string strTab = null;

                    if ((5 < arrLine_A.Length) &&
                        arrLine_A[5].Contains("Trailing whitespace") &&
                        DateTime.TryParse(arrLine_A[1], out dtParse))
                    {
                        strTab = "\t";
                    }

                    file_out.WriteLine(FormatLine(bAtErrors ? ksLineType_ErrorFile : ksLineType_File, nLineNo, strTab + strLine));
                    continue;
                }
                
                if (false == strDir.Contains(@":\"))
                {
                    Util.Assert(99949, false);        // all that's left is directories
                    continue;
                }

                // directory
                file_out.WriteLine(FormatLine(bAtErrors ? ksLineType_ErrorDir : ksLineType_Directory,
                    nLineNo, strLine.Replace(@"\\", @"\")));
            }
        }

        static string FormatLine(string strLineType, long nLineNo, string strLine_in = null) =>
            strLineType + "\t" + nLineNo + '\t' + ("" + strLine_in).TrimEnd();

        static internal string FormatString(string strDir = null, string strFile = null,
            DateTime? dtCreated = null, DateTime? dtModified = null,
            string strAttributes = null, ulong nLength = ulong.MaxValue,
            string strError1 = null, string strError2 = null, int? nHeader = null,
            string strHashV1pt0 = null, string strHashV2 = null)
        {
            var strCreated = "" + dtCreated;
            var strModified = "" + dtModified;

            var strLength =
                (ulong.MaxValue != nLength)
                ? "" + nLength
                : null;

            if (string.IsNullOrWhiteSpace(strDir + strFile + strCreated + strModified +
                strAttributes + strLength + strError1 + strError2 + strHashV1pt0 + strHashV2))
            {
                if (0 == nHeader)
                {
                    return "2" + '\t' + "3" + '\t' + "4" + '\t' + "5" + '\t' +
                        "6" + '\t' + "7" + '\t' +
                        "8" + '\t' + "9" + '\t' + "10" + '\t' + "11";
                }

                return "Dir" + '\t' + "File" + '\t' + "Created" + '\t' + "Modded" +'\t' +
                    "Attrib" + '\t' + "Length" + '\t' +
                    "Error1" + '\t' + "Error2" + '\t' +
                    "4K Hash" + '\t' + "1MB Hash";
            }

            var bDbgCheck = false;

            if ((("" + strDir).TrimEnd() != "" + strDir) ||
                (("" + strFile).TrimEnd() != "" + strFile))
            {
                strError1 += " Trailing whitespace";

                Util.Assert(99948, (false == string.IsNullOrWhiteSpace(strDir)) ||
                    (false == string.IsNullOrWhiteSpace(strFile)));

                bDbgCheck = true;
            }

            var strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' +
                strAttributes + '\t' + strLength + '\t' +
                strError1 + '\t' + strError2 + '\t' +
                strHashV1pt0 + '\t' + strHashV2).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                var asLine = strRet.Split('\t');
                var dtParse = DateTime.MinValue;

                if (asLine[knColLength01].Contains("Trailing whitespace") && DateTime.TryParse(asLine[1], out dtParse))
                    Util.Assert(99947, false);
#endif
            }

            return strRet;
        }

        static internal bool ReadHeader(string strFile, ref LVitem_ProjectVM lvItem_out)
        {
            var tupleValidate = ValidateFile(strFile);

            if (false == tupleValidate.Item1)
                return false;

            var lvItem = new LVitem_ProjectVM(lvItem_out)
            {
                ListingFile = strFile,
                Status = lvItem_out?.Status ?? ksUsingFile,
                Include = lvItem_out?.Include ?? true
            };

            const int knLinesDesired = 4 + knDriveInfoItems;
            const int knLinesGrabFile = 10;

            var asLines = strFile.ReadLines(99648).Take(knLinesDesired + knLinesGrabFile)
                .ToArray();

            if (knLinesDesired > asLines.Length)
                return false;

            if (false == asLines[1].StartsWith(ksLineType_Nickname))
                return false;

            var arrLine = asLines[1].Split('\t');

            if (2 < arrLine.Length)
                lvItem.Nickname = arrLine[2];

            if (false == asLines[2].StartsWith(ksLineType_Path))
                return false;

            if (null == (lvItem.SourcePath = ReadAttribute(asLines[2])))
                return false;

            asLines
                .Skip(4)
                .Where(s => s.StartsWith(ksLineType_VolumeInfo_DriveModel))
                .FirstOnlyAssert(s => lvItem.DriveModel = ReadAttribute(s));

            asLines
                .Skip(4)
                .Where(s => s.StartsWith(ksLineType_VolumeInfo_DriveSerial))
                .FirstOnlyAssert(s => lvItem.DriveSerial = ReadAttribute(s));

            lvItem.HashV2 =
                ksHeader != asLines.FirstOrDefault().Split('\t').Skip(2).FirstOrDefault()
                ? false
                : asLines
                .Where(strLine => strLine.StartsWith(ksLineType_File))
                .Select(strLine => strLine.Split('\t'))
                .Any(asLine => 11 < asLine.Length);

            lvItem.ScannedLength = "" + tupleValidate.Item2;
            lvItem.LinesTotal = tupleValidate.Item3;
            lvItem_out = lvItem;
            return true;
        }

        static string ReadAttribute(string s)
        {
            var astr = s.Split('\t');

            if (3 > astr.Length)
            {
                Util.Assert(99946, false);
                return null;
            }

            return astr[2];
        }

        static internal string StrFile_01(string strFile) =>
            Path.Combine(Path.GetDirectoryName(strFile),
            Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));

        static internal Tuple<bool, ulong, int>
            ValidateFile(string strFile)
        {
            var retVal = Tuple.Create(false, 0UL, 0);

            if (false == strFile.FileExists())
                return retVal;

            var arrLine =
                strFile.ReadLines(99647)
                .Take(1)
                .ToArray();

            if (0 == arrLine.Length)
                return retVal;

            var bConvertFile = false;

            if (arrLine[0] == ksHeader01)
            {
                Util.WriteLine("Converting " + strFile);
                ConvertFile(strFile);
                Util.WriteLine("File converted to " + ksHeader);
                bConvertFile = true;
            }

            switch (
                strFile.ReadLines(99646)
                .FirstOrDefault()
                .Split('\t')
                .Skip(2)
                .FirstOrDefault())
            {
                case ksHeader02:
                {
                    if (0 == _bWarned02)
                    {
                        Interlocked.Increment(ref _bWarned02);      // has to come before message due to parallel

                        //if (1 == _bWarned02)
                        //    MBoxStatic.ShowOverlay("Accuracy is increased if you re-scan drives using this build.");
                    }

                    break;
                }

                case ksHeader:
                {
                    break;
                }

                default:
                {
                    return retVal;
                }
            }

            var bRet = false;

            ulong nScannedLength = 0;
            var nLinesTotal = 0;

            strFile.ReadLines(99645)
                .Where(strLine => strLine.StartsWith(ksLineType_Length))
                .Select(strLine => strLine.Split('\t'))
                .FirstOnlyAssert(asLine =>
            {
                if ((3 < asLine.Length) && (ksTotalLengthLoc == asLine[2]))
                    try
                {
                    nScannedLength = ("" + asLine[knColLength]).ToUlong();
                    nLinesTotal = (asLine[1]).ToInt();
                    bRet = true;
                }
                catch { }
            });

            if (false == bRet)
                return retVal;

            var strFile_01 = StrFile_01(strFile);

            if (bConvertFile && LocalIsoStore.FileExists(strFile_01))
                LocalIsoStore.DeleteFile(strFile_01);

            return Tuple.Create(true, nScannedLength, nLinesTotal);
        }

        static internal bool
            IsAnyVolumeV2_BadHash => 0 < _bWarned02;
        static int
            _bWarned02 = 0;
    }
}
