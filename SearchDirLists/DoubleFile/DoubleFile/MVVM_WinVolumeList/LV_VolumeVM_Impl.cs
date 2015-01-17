using System.IO;
using System.Linq;

namespace DoubleFile
{
    partial class LV_VolumeVM
    {
        internal bool AlreadyInProject(LVitem_VolumeVM lvCurrentItem, string strFilename = null)
        {
            bool bAlreadyInProject = (ContainsListingFile(lvCurrentItem, strFilename) != null);

            if (bAlreadyInProject)
            {
                MBox.ShowDialog("Listing file already in the project.", "Add Listing File");
            }

            return bAlreadyInProject;
        }

        internal LVitem_VolumeVM ContainsListingFile(LVitem_VolumeVM currentItem, string t = null)
        {
            string s = (t ?? currentItem.ListingFile).ToLower();

            foreach (LVitem_VolumeVM item in m_items)
            {
                if ((item.ListingFile.ToLower() == s) &&
                    ((t == null) || (currentItem != item)))
                {
                    return item;
                }
            }

            return null;
        }

        internal void EditListingFile()
        {
            Selected().FirstOnlyAssert(lvItem =>
            {
                LVitem_VolumeVM lvItemVolumeTemp = new LVitem_VolumeVM(lvItem);

                while (true)
                {
                    WinVolumeEditBase dlg = SaveDirListings.WontSave(lvItem) ?
                        (WinVolumeEditBase)new WinVolumeEdit() :
                        new WinVolumeNew();

                    dlg.LVitemVolumeTemp = new LVitem_VolumeVM(lvItemVolumeTemp);

                    if (false == (dlg.ShowDialog(GetWindow()) ?? false))
                    {
                        // user cancelled
                        break;
                    }

                    lvItemVolumeTemp = new LVitem_VolumeVM(dlg.LVitemVolumeTemp);

                    if (false == AlreadyInProject(lvItem, lvItemVolumeTemp.ListingFile))
                    {
                        if (dlg is WinVolumeEdit)
                        {
                            if (ModifyListingFile(lvItem, lvItemVolumeTemp, (dlg as WinVolumeEdit).uc_VolumeEdit.DriveLetter))
                            {
                                //if (Utilities.MBox("Update the volume list?", "Modify file", MBoxBtns.YesNo) == MBoxRet.Yes)
                                //{
                                //    form_btnSaveVolumeList_Click();
                                //}
                                FileParse.ReadHeader(lvItemVolumeTemp.ListingFile, out lvItemVolumeTemp);
                            }
                        }
                        else if (FileExists(lvItemVolumeTemp.ListingFile))
                        {
                            continue;
                        }

                        lvItem.StringValues = lvItemVolumeTemp.StringValues;
                        break;
                    }
                }
            });
        }

        internal bool FileExists(string strListingFile)
        {
            bool bFileExists = System.IO.File.Exists(strListingFile) &&
                (false == strListingFile.StartsWith(ProjectFile.TempPath) || FileParse.ValidateFile(strListingFile));

            if (bFileExists)
            {
                MBox.ShowDialog("Listing file exists. Please manually delete it using the Save As dialog\nby clicking the icon button after this alert closes.", "New Listing File");
            }

            return bFileExists;
        }

        internal bool NewItem(LVitem_VolumeVM lvItem, bool bQuiet = false)
        {
            return NewItem(lvItem.StringValues, bQuiet);
        }

        internal override bool NewItem(string[] arrStr, bool bQuiet = false)
        {
            var lvItem = new LVitem_VolumeVM(arrStr);
            var bAlreadyInProject = AlreadyInProject(lvItem);

            if (false == bAlreadyInProject)
            {
                Add(lvItem, bQuiet);
            }

            return (false == bAlreadyInProject);
        }

        internal void RemoveListingFile()
        {
            bool bUnsaved = false;

            Selected().ToArray().ForEach(lvItem =>
            {
                if (bUnsaved)
                {
                    return;     // from lambda
                }

                if (false == SaveDirListings.WontSave(lvItem))
                {
                    bUnsaved = true;
                }
            });

            if (bUnsaved && (MBox.ShowDialog("Selected listings have not been saved. Continue?", "Remove Listing File",
                System.Windows.MessageBoxButton.YesNo) !=
                System.Windows.MessageBoxResult.Yes))
            {
                return;
            }

            Selected().ToArray().ForEach(lvItem => { Items.Remove(lvItem); });
        }

        internal void EditVolumeGroupLabel()
        {
            var dlg = new WinVolumeGroup();

            Selected().ToArray().FirstOnly(lvItem =>
            {
                dlg.Text = lvItem.VolumeGroup;
            });

            if (dlg.ShowDialog(GetWindow()) ?? false)
            {
                Selected().ToArray().ForEach(lvItem =>
                {
                    lvItem.VolumeGroup = dlg.Text;
                });
            }
        }

        internal void ToggleInclude()
        {
            Selected().ForEach(lvItem => { lvItem.Include = (false == lvItem.Include); });
        }

        bool ModifyListingFile(LVitem_VolumeVM origLVitemVolume, LVitem_VolumeVM lvItemVolumeTemp, char driveLetter)
        {
            if (false == FileParse.ValidateFile(origLVitemVolume.ListingFile))
            {
                MBox.ShowDialog("Bad listing file.", "Edit Listing File");
                return false;
            }

            var bDriveModel_Todo = ((origLVitemVolume.DriveModel ?? "") != (lvItemVolumeTemp.DriveModel ?? ""));
            var bDriveSerial_Todo = ((origLVitemVolume.DriveSerial ?? "") != (lvItemVolumeTemp.DriveSerial ?? ""));
            var bNickname_Todo = ((origLVitemVolume.Nickname ?? "") != (lvItemVolumeTemp.Nickname ?? ""));

            driveLetter = char.ToUpper(driveLetter);

            var driveLetterOrig = origLVitemVolume.SourcePath.ToUpper()[0];
            var bDriveLetter_Todo = (char.IsLetter(driveLetter) && (driveLetter != driveLetterOrig));

            if (false == (bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo || bDriveLetter_Todo))
            {
                return false;
            }

            var sbOut = new System.Text.StringBuilder();

            using (var reader = new StringReader(File.ReadAllText(origLVitemVolume.ListingFile)))
            {
                string strLine = null;

                while ((bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo) &&
                    (strLine = reader.ReadLine()) != null)
                {
                    if (strLine.StartsWith(FileParse.mSTRlineType_Start))
                    {
                        // already past the header: nothing left to replace.
                        MBox.Assert(0, false);
                        sbOut.AppendLine(strLine);
                        break;
                    }

                    var sbLine = new System.Text.StringBuilder(strLine);

                    var Replace = new System.Action<string>((s) =>
                    {
                        var astr = sbLine.ToString().Split('\t').ToList();

                        MBox.Assert(1308.9312, astr.Count == 3);

                        while (astr.Count < 3)
                        {
                            astr.Add("");
                        }

                        astr[2] = s;
                        sbLine = new System.Text.StringBuilder(string.Join("\t", astr));
                    });

                    if (bDriveModel_Todo &&
                        strLine.StartsWith(FileParse.mSTRlineType_VolumeInfo_DriveModel))
                    {
                        Replace(lvItemVolumeTemp.DriveModel);
                        bDriveModel_Todo = false;
                    }
                    else if (bDriveSerial_Todo &&
                        strLine.StartsWith(FileParse.mSTRlineType_VolumeInfo_DriveSerial))
                    {
                        Replace(lvItemVolumeTemp.DriveSerial);
                        bDriveSerial_Todo = false;
                    }
                    else if (bNickname_Todo &&
                        strLine.StartsWith(FileParse.mSTRlineType_Nickname))
                    {
                        Replace(lvItemVolumeTemp.Nickname);
                        bNickname_Todo = false;
                    }

                    sbOut.AppendLine(sbLine.ToString());
                }

                sbOut.Append(reader.ReadToEnd());

                if (bDriveLetter_Todo)
                {
                    sbOut.Replace("\t" + driveLetterOrig + @":\", "\t" + driveLetter + @":\");
                    bDriveLetter_Todo = false;
                }
            }

            File.WriteAllText(origLVitemVolume.ListingFile, sbOut.ToString());
            MBox.Assert(0, (false == (bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo || bDriveLetter_Todo)));
            return true;
        }
    }
}
