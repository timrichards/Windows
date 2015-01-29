using System.IO;
using System.Linq;

namespace DoubleFile
{
    partial class LV_ProjectVM
    {
        internal bool Unsaved { get; set; }

        internal LV_ProjectVM(LV_ProjectVM lvProjectVM_in = null)
        {
            if (lvProjectVM_in != null)
            {
                foreach (var lvItemVM in lvProjectVM_in.ItemsCast)
                {
                    Add(lvItemVM, bQuiet: true);
                }

                Unsaved = lvProjectVM_in.Unsaved;
            }
        }

        internal bool AlreadyInProject(LVitem_ProjectVM lvCurrentItem, string strFilename = null)
        {
            bool bAlreadyInProject = (ContainsListingFile(lvCurrentItem, strFilename) != null);

            if (bAlreadyInProject)
            {
                MBox.ShowDialog("Listing file already in the project.", "Add Listing File");
            }

            return bAlreadyInProject;
        }

        internal void EditListingFile()
        {
            Selected().FirstOnlyAssert(lvItem =>
            {
                LVitem_ProjectVM lvItemVolumeTemp = new LVitem_ProjectVM(lvItem);

                if (lvItemVolumeTemp.Status == FileParse.ksError)
                {
                    lvItemVolumeTemp.Status = FileParse.ksNotSaved;
                }

                while (true)
                {
                    WinVolumeEditBase dlg = lvItemVolumeTemp.WouldSave ?
                        new WinVolumeNew() :
                        (WinVolumeEditBase)new WinVolumeEdit();

                    dlg.LVitemVolumeTemp = new LVitem_ProjectVM(lvItemVolumeTemp);

                    if (false == (dlg.ShowDialog() ?? false))
                    {
                        // user cancelled
                        break;
                    }

                    lvItemVolumeTemp = new LVitem_ProjectVM(dlg.LVitemVolumeTemp);

                    if (false == AlreadyInProject(lvItem, lvItemVolumeTemp.ListingFile))
                    {
                        if (dlg is WinVolumeEdit)
                        {
                            if (ModifyListingFile(lvItem, lvItemVolumeTemp, (dlg as WinVolumeEdit).uc_VolumeEdit.DriveLetter))
                            {
                                //if (MBox.ShowDialog("Update the project?", "Modify file", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                                //{
                                //    form_btnSaveProject_Click();
                                //}
                                FileParse.ReadHeader(lvItemVolumeTemp.ListingFile, out lvItemVolumeTemp);
                                lvItem.StringValues = lvItemVolumeTemp.StringValues;
                                Unsaved = true;
                            }
                        }
                        else if (FileExists(lvItemVolumeTemp.ListingFile))
                        {
                            continue;
                        }

                        break;
                    }
                }
            });
        }

        internal bool FileExists(string strListingFile)
        {
            var bFileExists = System.IO.File.Exists(strListingFile) &&
                (false == strListingFile.StartsWith(ProjectFile.TempPath) || FileParse.ValidateFile(strListingFile));

            if (bFileExists)
            {
                MBox.ShowDialog("Listing file exists. Please manually delete it using the Save Listing File dialog\nby clicking the icon button after this alert closes.", "New Listing File");
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

                if (lvItem.WouldSave)
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
            Unsaved = true;
        }

        internal void EditVolumeGroupLabel()
        {
            var dlg = new WinVolumeGroup();

            Selected().ToArray().FirstOnly(lvItem =>
            {
                dlg.Text = lvItem.VolumeGroup;
            });

            if (dlg.ShowDialog() ?? false)
            {
                Selected().ToArray().ForEach(lvItem =>
                {
                    lvItem.VolumeGroup = dlg.Text;
                });

                Unsaved = true;
            }
        }

        internal void ToggleInclude()
        {
            Selected().ForEach(lvItem => { lvItem.Include = (false == lvItem.Include); });
            Unsaved = true;
        }

        LVitem_ProjectVM ContainsListingFile(LVitem_ProjectVM lvItem_Current, string t = null)
        {
            if (string.IsNullOrEmpty(t))
            {
                return null;
            }

            string s = (t ?? lvItem_Current.ListingFile).ToLower();

            foreach (LVitem_ProjectVM item in m_items)
            {
                if ((item.ListingFile.ToLower() == s) &&
                    ((t == null) || (lvItem_Current != item)))
                {
                    return item;
                }
            }

            return null;
        }

        bool ModifyListingFile(LVitem_ProjectVM lvItem_Orig, LVitem_ProjectVM lvItemVolumeTemp, char driveLetter)
        {
            if (false == FileParse.ValidateFile(lvItem_Orig.ListingFile))
            {
                MBox.ShowDialog("Bad listing file.", "Edit Listing File");
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

            var sbOut = new System.Text.StringBuilder();

            using (var reader = new StringReader(File.ReadAllText(lvItem_Orig.ListingFile)))
            {
                string strLine = null;

                while ((bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo) &&
                    (strLine = reader.ReadLine()) != null)
                {
                    if (strLine.StartsWith(FileParse.ksLineType_Start))
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

                sbOut.Append(reader.ReadToEnd());

                if (bDriveLetter_Todo)
                {
                    sbOut.Replace("\t" + driveLetterOrig + @":\", "\t" + driveLetter + @":\");
                    bDriveLetter_Todo = false;
                }
            }

            File.WriteAllText(lvItem_Orig.ListingFile, sbOut.ToString());
            MBox.Assert(0, (false == (bDriveModel_Todo || bDriveSerial_Todo || bNickname_Todo || bDriveLetter_Todo)));
            return true;
        }
    }
}
