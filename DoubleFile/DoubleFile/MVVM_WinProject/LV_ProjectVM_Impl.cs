using System;
using System.IO;
using System.Linq;

namespace DoubleFile
{
    partial class LV_ProjectVM
    {
        internal bool Unsaved { get; set; }

        internal LV_ProjectVM(GlobalData_Base gd_in = null, LV_ProjectVM lvProjectVM_in = null)
        {
            _gd = gd_in;

            if (lvProjectVM_in != null)
            {
                if (_gd == null)
                {
                    _gd = lvProjectVM_in._gd;
                }

                foreach (var lvItemVM in lvProjectVM_in.ItemsCast)
                {
                    Add(new LVitem_ProjectVM(lvItemVM), bQuiet: true);
                }

                Unsaved = lvProjectVM_in.Unsaved;
            }
        }

        internal bool AlreadyInProject(LVitem_ProjectVM lvCurrentItem, string strFilename = null)
        {
            bool bAlreadyInProject = (ContainsListingFile(lvCurrentItem, strFilename) != null);

            if (bAlreadyInProject)
            {
                MBoxStatic.ShowDialog("Listing file already in the project.", "Add Listing File");
            }

            return bAlreadyInProject;
        }

        internal void EditListingFile()
        {
            Selected().FirstOnlyAssert(lvItem =>
            {
                var lvItemVolumeTemp = new LVitem_ProjectVM(lvItem);

                if (lvItemVolumeTemp.Status == FileParse.ksError)
                {
                    lvItemVolumeTemp.Status = FileParse.ksNotSaved;
                }

                while (true)
                {
                    var dlg = lvItemVolumeTemp.WouldSave ?
                        new WinVolumeNew() :
                        (WinVolumeEditBase)new WinVolumeEdit();

                    dlg.LVitemVolumeTemp = new LVitem_ProjectVM(lvItemVolumeTemp);

                    if (false == (dlg.ShowDialog() ?? false))
                    {
                        // user cancelled
                        break;
                    }

                    lvItemVolumeTemp = new LVitem_ProjectVM(dlg.LVitemVolumeTemp);

                    if (AlreadyInProject(lvItem, lvItemVolumeTemp.ListingFile))
                    {
                        continue;
                    }

                    var dlgEdit = dlg as WinVolumeEdit;

                    if (dlgEdit != null)
                    {
                        if (ModifyListingFile(lvItem, lvItemVolumeTemp, dlgEdit.uc_VolumeEdit.DriveLetter))
                        {
                            FileParse.ReadHeader(lvItemVolumeTemp.ListingFile, out lvItemVolumeTemp);
                        }
                        else if (lvItem.Equals(lvItemVolumeTemp))
                        {
                            // volume group; include y/n: columns that aren't in the listing file
                            // no change
                            break;
                        }

                        Unsaved = true;
                    }
                    else    // WinVolumeNew
                    {
                        if (FileExists(lvItemVolumeTemp.ListingFile))
                            continue;
                    }

                    lvItem.StringValues = lvItemVolumeTemp.StringValues;
                    break;
                }
            });
        }

        internal bool FileExists(string strListingFile)
        {
            var bFileExists = File.Exists(strListingFile) &&
                (false == strListingFile.StartsWith(ProjectFile.TempPath) ||
                    FileParse.ValidateFile(strListingFile));

            if (bFileExists)
            {
                MBoxStatic.ShowDialog("Listing file exists. Please manually delete it using the Save Listing\n" +
                    "File dialog by clicking the icon button after this alert closes.", "New Listing File");
            }

            return bFileExists;
        }

        internal bool NewItem(LVitem_ProjectVM lvItem, bool bQuiet = false)
        {
            return NewItem(lvItem.StringValues, bQuiet);
        }

        internal override bool NewItem(string[] arrStr, bool bQuiet = false)
        {
            var lvItem = new LVitem_ProjectVM(arrStr);
            var bNotInProject = (false == AlreadyInProject(lvItem));

            if (bNotInProject)
            {
                Add(lvItem, bQuiet);
            }

            return (bNotInProject);
        }

        internal void RemoveListingFile()
        {
            var bUnsaved = false;

            Selected()
                .ForEach(lvItem =>
            {
                if (bUnsaved)
                {
                    return;     // from lambda
                }

                if (lvItem.WouldSave)
                {
                    bUnsaved = true;
                }
            });

            if (bUnsaved && (MBoxStatic.ShowDialog("Selected listings have not been saved. Continue?", "Remove Listing File",
                System.Windows.MessageBoxButton.YesNo) !=
                System.Windows.MessageBoxResult.Yes))
            {
                return;
            }

            Selected()
                .ToList()
                .ForEach(lvItem => Items.Remove(lvItem));
            Unsaved = (false == Items.IsEmpty());
        }

        internal void EditVolumeGroupLabel()
        {
            var dlg = new WinVolumeGroup();

            Selected()
                .First(lvItem =>
            {
                dlg.Text = lvItem.VolumeGroup;
            });

            if (dlg.ShowDialog() ?? false)
            {
                var strLabel = dlg.Text.Trim();

                Unsaved =
                    Selected()
                    .Aggregate(Unsaved, (current, lvItem) => 
                {
                    var bRet =
                        false == (lvItem.VolumeGroup ?? "").Equals(strLabel);

                    lvItem.VolumeGroup = dlg.Text;
                    return bRet || current;
                });
            }
        }

        internal void ToggleInclude()
        {
            Selected()
                .ForEach(lvItem => { lvItem.Include = (false == lvItem.Include); });
            Unsaved = true;
        }

        LVitem_ProjectVM ContainsListingFile(LVitem_ProjectVM lvItem_Current, string t)
        {
            if (string.IsNullOrEmpty(t))
            {
                return null;
            }

            var s = t.ToLower();

            return ItemsCast
                .FirstOrDefault(item =>
                    (item.ListingFile.ToLower() == s) &&
                    (lvItem_Current != item));
        }

        bool ModifyListingFile(LVitem_ProjectVM lvItem_Orig, LVitem_ProjectVM lvItemVolumeTemp, char driveLetter)
        {
            if (false == FileParse.ValidateFile(lvItem_Orig.ListingFile))
            {
                MBoxStatic.ShowDialog("Bad listing file.", "Edit Listing File");
                return false;
            }

            var bDriveModel_Todo = ((lvItem_Orig.DriveModel ?? "") != (lvItemVolumeTemp.DriveModel ?? ""));
            var bDriveSerial_Todo = ((lvItem_Orig.DriveSerial ?? "") != (lvItemVolumeTemp.DriveSerial ?? ""));
            var bNickname_Todo = ((lvItem_Orig.Nickname ?? "") != (lvItemVolumeTemp.Nickname ?? ""));

            driveLetter = char.ToUpper(driveLetter);

            var driveLetterOrig = lvItem_Orig.SourcePath.ToUpper()[0];
            var bDriveLetter_Todo = (char.IsLetter(driveLetter) && (driveLetter != driveLetterOrig));

            if (false == (bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo || bDriveLetter_Todo))
            {
                return false;
            }

            var strFile_01 = FileParse.StrFile_01(lvItem_Orig.ListingFile);

            if (File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            File.Move(lvItem_Orig.ListingFile, strFile_01);

            var sbOut = new System.Text.StringBuilder();

            using (var reader = File.OpenText(strFile_01))
            {
                string strLine = null;

                while ((bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo) &&
                    (strLine = reader.ReadLine()) != null)
                {
                    if (strLine.StartsWith(FileParse.ksLineType_Start))
                    {
                        // already past the header: nothing left to replace.
                        MBoxStatic.Assert(99989, false);
                        sbOut.AppendLine(strLine);
                        break;
                    }

                    var sbLine = new System.Text.StringBuilder(strLine);

                    System.Action<string> Replace = s =>
                    {
                        var astr = sbLine.ToString().Split('\t').ToList();

                        MBoxStatic.Assert(1308.9312, astr.Count == 3);

                        while (astr.Count < 3)
                        {
                            astr.Add("");
                        }

                        astr[2] = s;
                        sbLine = new System.Text.StringBuilder(string.Join("\t", astr));
                    };

                    if (bDriveModel_Todo &&
                        strLine.StartsWith(FileParse.ksLineType_VolumeInfo_DriveModel))
                    {
                        Replace(lvItemVolumeTemp.DriveModel);
                        bDriveModel_Todo = false;
                    }
                    else if (bDriveSerial_Todo &&
                        strLine.StartsWith(FileParse.ksLineType_VolumeInfo_DriveSerial))
                    {
                        Replace(lvItemVolumeTemp.DriveSerial);
                        bDriveSerial_Todo = false;
                    }
                    else if (bNickname_Todo &&
                        strLine.StartsWith(FileParse.ksLineType_Nickname))
                    {
                        Replace(lvItemVolumeTemp.Nickname);
                        bNickname_Todo = false;
                    }

                    sbOut.AppendLine(sbLine.ToString());
                }

                if (sbOut.Length > 0)
                {
                    File.WriteAllText(lvItem_Orig.ListingFile, sbOut.ToString());
                }
                else
                {
                    MBoxStatic.Assert(99988, false == File.Exists(lvItem_Orig.ListingFile));
                    try
                    {
                        File.Delete(lvItem_Orig.ListingFile);
                    }
                    catch (System.Exception e)
                    {
                        MBoxStatic.ShowDialog("Bad listing file.\n" + (e.GetBaseException() ?? e.InnerException ?? e).Message, "Edit Listing File");
                        return false;
                    }
                }

                using (var fileWriter = File.AppendText(lvItem_Orig.ListingFile))
                {
                    const int kBufSize = 1024 * 1024 * 4;
                    var buffer = new char[kBufSize];
                    var nRead = 0;

                    while ((nRead = reader.Read(buffer, 0, kBufSize)) > 0)
                    {
                        sbOut.Clear();
                        sbOut.Append(buffer, 0, nRead);

                        if (bDriveLetter_Todo)
                        {
                            sbOut.Replace("\t" + driveLetterOrig + @":\", "\t" + driveLetter + @":\");
                        }

                        fileWriter.Write(sbOut.ToString());
                    }
                }

                bDriveLetter_Todo = false;
            }

            MBoxStatic.Assert(99987, (false == (bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo || bDriveLetter_Todo)));
            return true;
        }

        GlobalData_Base _gd = null;
    }
}
