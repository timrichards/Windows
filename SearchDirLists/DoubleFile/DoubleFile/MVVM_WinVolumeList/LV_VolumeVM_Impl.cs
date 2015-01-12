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
                            ModifyFile(lvItem, lvItemVolumeTemp, (dlg as WinVolumeEdit).uc_VolumeEdit.DriveLetter);
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
            bool bFileExists = System.IO.File.Exists(strListingFile);

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

        internal void SetVolumeGroup() { MBox.ShowDialog("SetVolumeGroup"); }

        internal void ToggleInclude()
        {
            Selected().ForEach(lvItem => { lvItem.Include = (false == lvItem.Include); });
        }

        void ModifyFile(LVitem_VolumeVM origLVitemVolume, LVitem_VolumeVM lvItemVolumeTemp, char driveLetter)
        {
            if (false == FileParse.ValidateFile(origLVitemVolume.ListingFile))
            {
                MBox.ShowDialog("Bad listing file.", "Edit Listing File");
                return;
            }

        // strings[0]
            // strLine.StartsWith(Utilities.mSTRlineType_Nickname)

            driveLetter = char.ToUpper(driveLetter);

            var bDriveLetter = (char.IsLetter(driveLetter) &&
                driveLetter != origLVitemVolume.SourcePath.ToUpper()[0]);
            //else if (bDriveLetter)
            //{
            //sbLine.Replace("\t" + strDriveLetter_orig + @":\", "\t" + strDriveLetter + @":\");
            //}

        // volume group is part of the project file, not the listing file

            // mSTRlineType_VolumeInfo_DriveModel
            // mSTRlineType_VolumeInfo_DriveSerial
        }

#if false
        internal void form_btnModifyFile_Click(object sender = null, EventArgs e = null)
        {
            gd.InterruptTreeTimerWithAction(new BoolAction(() =>
            {
                ListView.SelectedListViewItemCollection lvSelect = form_lvVolumesMain.SelectedItems;

                if (lvSelect.Count <= 0)
                {
                    return false;
                }

                if (lvSelect.Count > 1)
                {
                    Utilities.Assert(1308.9311, false, bTraceOnly: true);    // guaranteed by selection logic
                    Utilities.MBox("Only one file can be modified at a time.", "Modify file");
                    return false;
                }

                string strVolumeName_orig = form_lvVolumesMain.SelectedItems[0].Text;
                string strVolumeName = null;
                string strFileName = form_lvVolumesMain.SelectedItems[0].SubItems[2].Text;

                try { using (new StreamReader(strFileName)) { } }
                catch
                {
                    if (gd.m_saveDirListings != null)
                    {
                        Utilities.MBox("Currently saving listings and can't open file yet. Please wait.", "Modify file");
                    }
                    else if (false == string.IsNullOrWhiteSpace(lvSelect[0].Name))
                    {
                        Utilities.MBox("File hasn't been saved yet.", "Modify file");
                    }
                    else
                    {
                        Utilities.MBox("Can't open file.", "Modify file");
                    }

                    return false;
                }

                {
                    InputBox inputBox = new InputBox();

                    inputBox.Text = "Step 1 of 2: Volume name";
                    inputBox.Prompt = "Enter a volume name. (Next: drive letter)";
                    inputBox.Entry = strVolumeName_orig;
                    inputBox.SetNextButtons();

                    if ((inputBox.ShowDialog() == DialogResult.OK) && (false == string.IsNullOrWhiteSpace(inputBox.Entry)))
                    {
                        strVolumeName = inputBox.Entry;
                    }
                }

                string strDriveLetter_orig = null;
                string strDriveLetter = null;

                while (true)
                {
                    InputBox inputBox = new InputBox();

                    inputBox.Text = "Step 2 of 2: Drive letter";
                    inputBox.Prompt = "Enter a drive letter.";

                    string str = form_lvVolumesMain.SelectedItems[0].SubItems[1].Text;

                    if (str.Length <= 0)
                    {
                        Utilities.Assert(1308.9312, false, bTraceOnly: true);
                        break;
                    }

                    strDriveLetter_orig = str[0].ToString();
                    inputBox.Entry = strDriveLetter_orig.ToUpper();
                    inputBox.SetNextButtons();

                    if (inputBox.ShowDialog() == DialogResult.OK)
                    {
                        if (inputBox.Entry.Length > 1)
                        {
                            Utilities.MBox("Drive letter must be one letter.", "Drive letter");
                            continue;
                        }

                        strDriveLetter = inputBox.Entry.ToUpper();
                    }

                    break;
                }

                if ((string.IsNullOrWhiteSpace(strVolumeName) ||
                    ((strVolumeName ?? "") == (strVolumeName_orig ?? "")))
                    &&
                    (string.IsNullOrWhiteSpace(strDriveLetter) ||
                    ((strDriveLetter ?? "") == (strDriveLetter_orig ?? ""))))
                {
                    Utilities.MBox("No changes made.", "Modify file");
                    return false;
                }

                StringBuilder sbFileConts = new StringBuilder();
                bool bDriveLetter = (false == string.IsNullOrWhiteSpace(strDriveLetter));

                gd.KillTreeBuilder(bJoin: true);

                using (StringReader reader = new StringReader(File.ReadAllText(strFileName)))
                {
                    string strLine = null;
                    bool bHitNickname = string.IsNullOrWhiteSpace(strVolumeName);

                    while ((strLine = reader.ReadLine()) != null)
                    {
                        StringBuilder sbLine = new StringBuilder(strLine);

                        if ((bHitNickname == false) && strLine.StartsWith(Utilities.mSTRlineType_Nickname))
                        {
                            if (false == string.IsNullOrWhiteSpace(strVolumeName_orig))
                            {
                                sbLine.Replace(strVolumeName_orig, (strVolumeName ?? ""));
                            }
                            else
                            {
                                Utilities.Assert(1308.9313, sbLine.ToString().Split('\t').Length == 2);
                                sbLine.Append('\t');
                                sbLine.Append(strVolumeName);
                            }

                            form_lvVolumesMain.SelectedItems[0].Text = strVolumeName;
                            bHitNickname = true;
                        }
                        else if (bDriveLetter)
                        {
                            sbLine.Replace("\t" + strDriveLetter_orig + @":\", "\t" + strDriveLetter + @":\");
                        }

                        sbFileConts.AppendLine(sbLine.ToString());
                    }
                }

                if (bDriveLetter)
                {
                    SDL_ListViewItem lvItem = (SDL_ListViewItem)form_lvVolumesMain.SelectedItems[0];

                    lvItem.SubItems[1].Text = (strDriveLetter + ":");
                }

                File.WriteAllText(strFileName, sbFileConts.ToString());
                gd.m_blinky.Go(form_btnSaveVolumeList);

                if (Utilities.MBox("Update the volume list?", "Modify file", MBoxBtns.YesNo) == MBoxRet.Yes)
                {
                    form_btnSaveVolumeList_Click();
                }

                return true;
            }));
        }
#endif
    }
}
