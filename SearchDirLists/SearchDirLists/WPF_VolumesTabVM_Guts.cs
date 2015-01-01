using System;
using System.IO;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace SearchDirLists
{
    partial class VolumesTabVM
    {
        void DoTree()
        {
            UList<LVvolStrings> list_lvVolStrings = new UList<LVvolStrings>();

            foreach (VolumeLVitemVM lvItem in LV.Items)
            {
                list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            gd.m_tree = new Tree(list_lvVolStrings, gd.m_dictNodes, gd.m_dictDriveInfo,
                new TreeStatusDelegate(TreeStatusCallback), new Action(TreeDoneCallback));
            gd.m_tree.DoThreadFactory();
        }

        internal void TreeStatusCallback(LVvolStrings volStrings, SDL_TreeNode rootNode = null, bool bError = false)
        {
            if (GlobalData.AppExit || (gd.m_tree == null) || (gd.m_tree.IsAborted))
            {
                gd.TreeCleanup();
                return;
            }

            if (m_app.CheckAccess() == false) { m_app.Dispatcher.Invoke(new TreeStatusDelegate(TreeStatusCallback), new object[] { volStrings, rootNode, bError }); return; }

            if (bError)
            {
     //           volStrings.SetStatus_BadFile(LV);
            }
            else if (rootNode != null)
            {
                //lock (TV)
                //{
                //    TV.Nodes.Add(rootNode.Text);    // items added to show progress
                //    volStrings.SetStatus_Done(LV, rootNode);
                //}

                lock (gd.m_listRootNodes)
                {
                    gd.m_listRootNodes.Add(rootNode);
                }
            }
            else
            {
                Utilities.Assert(1304.5309, false);
            }
        }

        void TreeDoneCallback()
        {
#if (WPF)
            Collate collate = new Collate(gd.m_dictNodes,
                m_app.BrowseTab.LV_Clones.data, m_app.BrowseTab.LV_SameVol.data, m_app.BrowseTab.LV_Solitary.data,
                gd.m_listRootNodes, gd.m_listTreeNodes, gd.m_bCheckboxes,
                gd.m_listLVignore, true);
            DateTime dtStart = DateTime.Now;

            collate.Step1_OnThread();
            Utilities.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;

            if (GlobalData.AppExit)
            {
                gd.TreeCleanup();
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            Utilities.CheckAndInvoke(m_app.Dispatcher, new Action(collate.Step2_OnForm));
            Utilities.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            collate = null;
            gd.TreeCleanup();
            GC.Collect();
#endif
            Utilities.CheckAndInvoke(m_app.Dispatcher, new Action(() =>
            {
                m_app.BrowseTab.TV.SetData(gd.m_listRootNodes);
                m_app.BrowseTab.LV_Clones.SyncData();
                m_app.BrowseTab.LV_SameVol.SyncData();
                m_app.BrowseTab.LV_Solitary.SyncData();

                CopyScratchpadListViewVM lvFake = new CopyScratchpadListViewVM(null);   // Hack: check changed event loads the real listviewer

            //    foreach (CopyScratchpadLVitemVM lvItem in m_Browse.LV_CopyScratchpad.Items)
            //    {
            //        lvFake.Items.Add(lvItem.Clone());
            //    }

            //    m_Browse.LV_CopyScratchpad.Items.Clear();
            //    m_Browse.LoadCopyScratchPad(lvFake);
            }));
        }

        void AddVolume()
        {
            gd.InterruptTreeTimerWithAction(new BoolAction(() =>
            {
                bool bSaveAsExists = false;

                if (SaveFields(false) == false)
                {
                    return false;
                }

                if (Utilities.StrValid(CB_SaveAs.Current) == false)
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                    return false;
                }

                if (LV.ContainsSaveAs(CB_SaveAs.Current))
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "File already in use in list of volumes.", "Volume Save As");
                    return false;
                }

                bool bOpenedFile = (Utilities.StrValid(CB_Path.Current) == false);

                if (File.Exists(CB_SaveAs.Current) && Utilities.StrValid(CB_Path.Current))
                {
                    gd.m_blinky.Go(m_app.xaml_cbSaveAs, clr: Drawing.Color.Red);

                    if (Utilities.MBox(CB_SaveAs + ("\nalready exists. Overwrite?").PadRight(100), "Volume Save As", MBoxBtns.YesNo)
                        != MBoxRet.Yes)
                    {
                        gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Yellow, Once: true);
                        m_app.xaml_cbVolumeName.Text = string.Empty;
                        gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Yellow, Once: true);
                        CB_Path.Current = string.Empty;
                        Utilities.Assert(1308.9306, SaveFields(false));
                    }
                }

                if ((File.Exists(CB_SaveAs.Current) == false) && (Utilities.StrValid(CB_Path.Current) == false))
                {
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red);
                    Utilities.MBox("Must have a path or existing directory listing file.", "Volume Source Path");
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red, Once: true);
                    return false;
                }

                if (Utilities.StrValid(CB_Path.Current) && (Directory.Exists(CB_Path.Current) == false))
                {
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red);
                    Utilities.MBox("Path does not exist.", "Volume Source Path");
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red, Once: true);
                    return false;
                }

                string strStatus = "Not Saved";

                if (File.Exists(CB_SaveAs.Current))
                {
                    if (Utilities.StrValid(CB_Path.Current) == false)
                    {
                        bSaveAsExists = ReadHeader();

                        //if (bSaveAsExists == false)
                        //{
                        //    Utilities.ConvertFile(CBSaveAs.S);
                        //    bSaveAsExists = ReadHeader();
                        //}

                        if (bSaveAsExists)
                        {
                            strStatus = Utilities.mSTRusingFile;
                        }
                        else
                        {
                            if (Utilities.StrValid(CB_Path.Current))
                            {
                                strStatus = "File is bad. Will overwrite.";
                            }
                            else
                            {
                                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red);
                                Utilities.MBox("File is bad and path does not exist.", "Volume Source Path");
                                gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red, Once: true);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        strStatus = "Will overwrite.";
                    }
                }

                if ((bSaveAsExists == false) && (LV.ContainsUnsavedPath(CB_Path.Current)))
                {
                    gd.FormError(m_app.xaml_cbPath, "Path already added.", "Volume Source Path");
                    return false;
                }

                if (Utilities.StrValid(CB_VolumeName.Current))
                {
                    if (LV.ContainsVolumeName(CB_VolumeName.Current))
                    {
                        gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Red);

                        if (Utilities.MBox("Nickname already in use. Use it for more than one volume?", "Volume Save As", MBoxBtns.YesNo)
                            != MBoxRet.Yes)
                        {
                            gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Red, Once: true);
                            return false;
                        }
                    }
                }
                else if (bOpenedFile == false)
                {
                    gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Red);

                    if (Utilities.MBox("Continue without entering a nickname for this volume?", "Volume Save As", MBoxBtns.YesNo)
                        != MBoxRet.Yes)
                    {
                        gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Red, Once: true);
                        return false;
                    }
                }

                LV.NewItem(new string[] { CB_VolumeName.Current, CB_Path.Current, CB_SaveAs.Current, strStatus, "Yes" });
                return bSaveAsExists;
            }));
        }

        string FormatPath(string strPath, Control ctl, bool bFailOnDirectory = true)
        {
            if (Directory.Exists(Path.GetFullPath(strPath)))
            {
                string strCapDrive = strPath.Substring(0, strPath.IndexOf(@":\") + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath != strCapDrive.ToUpper())
                {
                    strPath = strPath.TrimEnd('\\');
                }
            }
            else if (bFailOnDirectory)
            {
                m_app.xaml_tabControlMain.SelectedItem = m_app.xaml_tabItemVolumes;
                gd.FormError(ctl, "Path does not exist.", "Save Fields");
                return null;
            }

            return strPath.TrimEnd('\\');
        }

        void LoadVolumeList_Click() { gd.InterruptTreeTimerWithAction(new BoolAction(() => { return LoadVolumeList(); })); DoTree(); }

        bool LoadVolumeList(string strFile = null)
        {
            if (new SDL_VolumeFile(strFile).ReadList(LV) == false)
            {
                return false;
            }

            if (LV.HasItems)
            {
                m_app.xaml_tabControlMain.SelectedItem = m_app.xaml_tabItemBrowse;
            }

            return true;    // this kicks off the tree
        }

        void ModifyFile()
        {
            gd.InterruptTreeTimerWithAction(new BoolAction(() =>
            {
                if (LV.SelectedAny == false)
                {
                    return false;
                }

                if (LV.Selected.Count() > 1)
                {
                    Utilities.Assert(1308.9311, false, bTraceOnly: true);    // guaranteed by selection logic
                    Utilities.MBox("Only one file can be modified at a time.", "Modify file");
                    return false;
                }

                VolumeLVitemVM lvItem = LV.Selected.First();

                string strVolumeName_orig = lvItem.VolumeName;
                string strVolumeName = null;
                string strSaveAs = lvItem.SaveAs;

                try { using (new StreamReader(strSaveAs)) { } }
                catch
                {
                    if (gd.m_saveDirListings != null)
                    {
                        Utilities.MBox("Currently saving listings and can't open file yet. Please wait.", "Modify file");
                    }
                    else if (lvItem.SaveAsExists == false)
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

                    if ((inputBox.ShowDialog() == Forms.DialogResult.OK) && (Utilities.StrValid(inputBox.Entry)))
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

                    if (lvItem.Path.Length <= 0)
                    {
                        Utilities.Assert(1308.9312, false, bTraceOnly: true);
                        break;
                    }

                    strDriveLetter_orig = lvItem.Path[0].ToString();
                    inputBox.Entry = strDriveLetter_orig.ToUpper();
                    inputBox.SetNextButtons();

                    if (inputBox.ShowDialog() == Forms.DialogResult.OK)
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

                if (((Utilities.StrValid(strVolumeName) == false) ||
                    (Utilities.NotNull(strVolumeName) == Utilities.NotNull(strVolumeName_orig)))
                    &&
                    ((Utilities.StrValid(strDriveLetter) == false) ||
                    (Utilities.NotNull(strDriveLetter) == Utilities.NotNull(strDriveLetter_orig))))
                {
                    Utilities.MBox("No changes made.", "Modify file");
                    return false;
                }

                StringBuilder sbFileConts = new StringBuilder();
                bool bDriveLetter = Utilities.StrValid(strDriveLetter);

                gd.KillTreeBuilder(bJoin: true);

                using (StringReader reader = new StringReader(File.ReadAllText(strSaveAs)))
                {
                    string strLine = null;
                    bool bHitNickname = (Utilities.StrValid(strVolumeName) == false);

                    while ((strLine = reader.ReadLine()) != null)
                    {
                        StringBuilder sbLine = new StringBuilder(strLine);

                        if ((bHitNickname == false) && strLine.StartsWith(Utilities.mSTRlineType_Nickname))
                        {
                            if (Utilities.StrValid(strVolumeName_orig))
                            {
                                sbLine.Replace(strVolumeName_orig, Utilities.NotNull(strVolumeName));
                            }
                            else
                            {
                                Utilities.Assert(1308.9313, sbLine.ToString().Split('\t').Length == 2);
                                sbLine.Append('\t');
                                sbLine.Append(strVolumeName);
                            }

                            lvItem.VolumeName = strVolumeName;
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
                    lvItem.Path = strDriveLetter + ":";
                }

                File.WriteAllText(strSaveAs, sbFileConts.ToString());
                gd.m_blinky.Go(m_app.xaml_btnSaveVolumeList);

                if (Utilities.MBox("Update the volume list?", "Modify file", MBoxBtns.YesNo) == MBoxRet.Yes)
                {
                    SaveVolumeList();
                }

                return true;
            }));
        }

        bool ReadHeader()
        {
            if (Utilities.ValidateFile(CB_SaveAs.Current) == false)
            {
                return false;
            }

            using (StreamReader file = new StreamReader(CB_SaveAs.Current))
            {
                string line = null;

                if ((line = file.ReadLine()) == null) return false;
                if ((line = file.ReadLine()) == null) return false;
                if (line.StartsWith(Utilities.mSTRlineType_Nickname) == false) return false;

                string[] arrLine = line.Split('\t');
                string strName = string.Empty;

                if (arrLine.Length > 2) strName = arrLine[2];
                CB_VolumeName.Current = strName;
                if ((line = file.ReadLine()) == null) return false;
                if (line.StartsWith(Utilities.mSTRlineType_Path) == false) return false;
                arrLine = line.Split('\t');
                if (arrLine.Length < 3) return false;
                CB_Path.Current = arrLine[2];
            }

            return SaveFields(false);
        }

        void RemoveVolume()
        {
            if (LV.SelectedAny == false)
            {
                return;
            }

            gd.m_bKillTree = (gd.m_tree != null) || (gd.m_bKillTree && gd.timer_DoTree.IsEnabled);
            gd.timer_DoTree.IsEnabled = false;
            gd.KillTreeBuilder(bJoin: true);

            if (gd.m_bKillTree == false)
            {
                uint nNumFoldersKeep = 0;
                uint nNumFoldersRemove = 0;

                foreach (VolumeLVitemVM lvItem in LV.ItemsCast)
                {
                    if (lvItem.treeNode == null)
                    {
                        // scenario: unsaved file
                        continue;
                    }

                    RootNodeDatum rootNodeDatum = (RootNodeDatum)lvItem.treeNode.Tag;

                    if (LV.Selected.Contains(lvItem))
                    {
                        nNumFoldersRemove += rootNodeDatum.nSubDirs;
                    }
                    else
                    {
                        nNumFoldersKeep += rootNodeDatum.nSubDirs;
                    }
                }

                gd.m_bKillTree = (nNumFoldersRemove > nNumFoldersKeep);
            }

            if (gd.m_bKillTree)
            {
                gd.RestartTreeTimer();
            }
            else
            {
                List<VolumeLVitemVM> listLVvolItems = new List<VolumeLVitemVM>();

                foreach (VolumeLVitemVM lvItem in LV.Selected)
                {
                    if (lvItem.treeNode == null)
                    {
                        // scenario: unsaved file
                        continue;
                    }

                    listLVvolItems.Add(lvItem);
                    SDLWPF.treeViewMain.Nodes.Remove(lvItem.treeNode);
                }

                new Thread(new ThreadStart(() =>
                {
                    foreach (VolumeLVitemVM lvItem in listLVvolItems)
                    {
                        gd.RemoveCorrelation(lvItem.treeNode);
                        gd.m_listRootNodes.Remove(lvItem.treeNode);
                    }

                    m_app.Dispatcher.Invoke(new Action(() =>
                    {
                        gd.RestartTreeTimer();
                    }));
                }))
                .Start();
            }

            foreach (VolumeLVitemVM lvItem in LV.Selected.ToArray())
            {
                LV.Items.Remove(lvItem);
            }
        }

        void SaveAs()
        {
            SDL_File.Init();
            SDL_File.SFD.Filter = SDL_File.FileAndDirListFileFilter + "|" + SDL_File.BaseFilter;

            if (Utilities.StrValid(CB_SaveAs.Current))
            {
                SDL_File.SFD.InitialDirectory = Path.GetDirectoryName(CB_SaveAs.Current);
            }

            if (SDL_File.SFD.ShowDialog() == Forms.DialogResult.OK)
            {
                CB_SaveAs.Current = SDL_File.SFD.FileName;

                if (File.Exists(CB_SaveAs.Current))
                {
                    CB_VolumeName.Current = null;
                    CB_Path.Current = null;
                }
            }
        }

        void SaveDirLists()
        {
            bool bRestartTreeTimer = gd.timer_DoTree.IsEnabled;

            gd.timer_DoTree.Stop();

            if ((gd.DoSaveDirListings(LV.ItemsCast, SaveDirListingsStatusCallback, SaveDirListingsDoneCallback)
                == false) && bRestartTreeTimer)   // cancelled
            {
                gd.RestartTreeTimer();
            }
        }

        bool SaveFields(bool bFailOnDirectory = true)
        {
            CB_VolumeName.Current = Utilities.NotNull(m_app.xaml_cbVolumeName.Text).Trim();
            CB_Path.Current = Utilities.NotNull(m_app.xaml_cbPath.Text).Trim();

            if (Utilities.StrValid(CB_Path.Current))
            {
                CB_Path.Current += '\\';

                string str = FormatPath(CB_Path.Current, m_app.xaml_cbPath, bFailOnDirectory);

                if (str != null)
                {
                    CB_Path.Current = str;
                }
                else
                {
                    return false;
                }
            }

            if (Utilities.StrValid(m_app.xaml_cbSaveAs.Text))
            {
                try
                {
                    CB_SaveAs.Current = Path.GetFullPath(m_app.xaml_cbSaveAs.Text.Trim());
                }
                catch
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Error in save listings filename.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(Path.GetDirectoryName(CB_SaveAs.Current)) == false)
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Directory to save listings to doesn't exist.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(CB_SaveAs.Current))
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Must specify save filename. Only directory entered.", "Save Fields");
                    return false;
                }

                string str = FormatPath(CB_SaveAs.Current, m_app.xaml_cbSaveAs, bFailOnDirectory);

                if (str != null)
                {
                    CB_SaveAs.Current = str;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        void SaveVolumeList()
        {
            if (LV.HasItems)
            {
                new SDL_VolumeFile().WriteList(LV.ItemsCast);
            }
            else
            {
                gd.m_blinky.Go(ctl: m_app.xaml_btnSaveVolumeList, clr: Drawing.Color.Red, Once: true);
                Utilities.Assert(1308.9314, false, bTraceOnly: true);    // shouldn't even be hit: this button gets dimmed
            }
        }

        void SetPath()
        {
            if (folderBrowserDialog1.ShowDialog() == Forms.DialogResult.OK)
            {
                CB_Path.Current = folderBrowserDialog1.SelectedPath;
            }
        }

        void SetVolumeGroup()
        {
            gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;

            gd.InterruptTreeTimerWithAction(new BoolAction(() =>
            {
                VolumeLVitemVM[] lvSelect = m_app.xaml_lvVolumesMain.SelectedItems.Cast<VolumeLVitemVM>().ToArray();

                if (lvSelect.Length <= 0)
                {
                    return false;
                }

                InputBox inputBox = new InputBox();

                inputBox.Text = "Volume Group";
                inputBox.Prompt = "Enter a volume group name";
                inputBox.Entry = lvSelect[0].VolumeGroup;

                SortedDictionary<string, object> dictVolGroups = new SortedDictionary<string, object>();

                foreach (VolumeLVitemVM lvItem in LV.ItemsCast)
                {
                    if ((lvItem.VolumeGroup != null) && (dictVolGroups.ContainsKey(lvItem.VolumeGroup) == false))
                    {
                        dictVolGroups.Add(lvItem.VolumeGroup, null);
                    }
                }

                foreach (KeyValuePair<string, object> entry in dictVolGroups)
                {
                    inputBox.AddSelector(entry.Key);
                }

                if (inputBox.ShowDialog() != Forms.DialogResult.OK)
                {
                    return false;
                }

                foreach (VolumeLVitemVM lvItem in lvSelect)
                {
                    lvItem.VolumeGroup = inputBox.Entry;

                    if (lvItem.treeNode == null)
                    {
                        gd.m_bKillTree = true;
                    }
                    else if (gd.m_bKillTree == false)
                    {
                        ((RootNodeDatum)(lvItem.treeNode).Tag).StrVolumeGroup = inputBox.Entry;
                    }
                }

                return true;
            }));
        }

        void ToggleInclude()
        {
            VolumeLVitemVM[] lvSelect = m_app.xaml_lvVolumesMain.SelectedItems.Cast<VolumeLVitemVM>().ToArray();

            if (lvSelect.Length > 0)
            {
                foreach (VolumeLVitemVM lvItem in lvSelect)
                {
                    lvItem.Include = (lvItem.Include == false);
                }

                gd.RestartTreeTimer();
            }
        }
    }
}
