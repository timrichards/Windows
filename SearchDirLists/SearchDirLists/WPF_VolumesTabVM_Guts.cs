﻿using System;
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
        void AddVolume()
        {
            gd.InterruptTreeTimerWithAction(new BoolAction(() =>
            {
                bool bSaveAsExists = false;

                if (SaveFields(false) == false)
                {
                    return false;
                }

                if (Utilities.StrValid(CBSaveAs.S) == false)
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Must have a file to load or save directory listing to.", "Volume Save As");
                    return false;
                }

                if (m_VolsLVvm.ContainsSaveAs(CBSaveAs.S))
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "File already in use in list of volumes.", "Volume Save As");
                    return false;
                }

                bool bOpenedFile = (Utilities.StrValid(CBPath.S) == false);

                if (File.Exists(CBSaveAs.S) && Utilities.StrValid(CBPath.S))
                {
                    gd.m_blinky.Go(m_app.xaml_cbSaveAs, clr: Drawing.Color.Red);

                    if (Utilities.MBox(CBSaveAs + ("\nalready exists. Overwrite?").PadRight(100), "Volume Save As", MBoxBtns.YesNo)
                        != MBoxRet.Yes)
                    {
                        gd.m_blinky.Go(m_app.xaml_cbVolumeName, clr: Drawing.Color.Yellow, Once: true);
                        m_app.xaml_cbVolumeName.Text = String.Empty;
                        gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Yellow, Once: true);
                        CBPath.S = String.Empty;
                        Utilities.Assert(1308.9306, SaveFields(false));
                    }
                }

                if ((File.Exists(CBSaveAs.S) == false) && (Utilities.StrValid(CBPath.S) == false))
                {
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red);
                    Utilities.MBox("Must have a path or existing directory listing file.", "Volume Source Path");
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red, Once: true);
                    return false;
                }

                if (Utilities.StrValid(CBPath.S) && (Directory.Exists(CBPath.S) == false))
                {
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red);
                    Utilities.MBox("Path does not exist.", "Volume Source Path");
                    gd.m_blinky.Go(m_app.xaml_cbPath, clr: Drawing.Color.Red, Once: true);
                    return false;
                }

                String strStatus = "Not Saved";

                if (File.Exists(CBSaveAs.S))
                {
                    if (Utilities.StrValid(CBPath.S) == false)
                    {
                        bSaveAsExists = ReadHeader();

                        if (bSaveAsExists)
                        {
                            strStatus = Utilities.mSTRusingFile;
                        }
                        else
                        {
                            if (Utilities.StrValid(CBPath.S))
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

                if ((bSaveAsExists == false) && (m_VolsLVvm.ContainsUnsavedPath(CBPath.S)))
                {
                    gd.FormError(m_app.xaml_cbPath, "Path already added.", "Volume Source Path");
                    return false;
                }

                if (Utilities.StrValid(CBVolumeName.S))
                {
                    if (m_VolsLVvm.ContainsVolumeName(CBVolumeName.S))
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

                m_VolsLVvm.Add(new VolumeLVitemVM(CBVolumeName.S, CBPath.S, CBSaveAs.S, strStatus, bSaveAsExists));
                return bSaveAsExists;
            }));
        }

        String FormatPath(String strPath, Control ctl, bool bFailOnDirectory = true)
        {
            if (Directory.Exists(Path.GetFullPath(strPath)))
            {
                String strCapDrive = strPath.Substring(0, strPath.IndexOf(":" + Path.DirectorySeparatorChar) + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath != strCapDrive.ToUpper())
                {
                    strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
                }
            }
            else if (bFailOnDirectory)
            {
                m_app.xaml_tabControlMain.SelectedItem = m_app.xaml_tabPageVolumes;
                gd.FormError(ctl, "Path does not exist.", "Save Fields");
                return null;
            }

            return strPath.TrimEnd(Path.DirectorySeparatorChar);
        }

        void LoadVolumeList_Click() { gd.InterruptTreeTimerWithAction(new BoolAction(() => { return LoadVolumeList(); })); }

        bool LoadVolumeList(String strFile = null)
        {
            if (new SDL_VolumeFile(strFile).ReadList(m_VolsLVvm.Items) == false)
            {
                return false;
            }

            if (HasItems)
            {
                m_app.xaml_tabControlMain.SelectedItem = m_app.xaml_tabPageBrowse;
            }

            return true;    // this kicks off the tree
        }

        void ModifyFile()
        {
            gd.InterruptTreeTimerWithAction(new BoolAction(() =>
            {
                VolumeLVitemVM[] lvSelect = m_app.xaml_lvVolumesMain.SelectedItems.Cast<VolumeLVitemVM>().ToArray();

                if (lvSelect.Length <= 0)
                {
                    return false;
                }

                if (lvSelect.Length > 1)
                {
                    Utilities.Assert(1308.9311, false, bTraceOnly: true);    // guaranteed by selection logic
                    Utilities.MBox("Only one file can be modified at a time.", "Modify file");
                    return false;
                }

                VolumeLVitemVM lvItem = lvSelect[0];

                String strVolumeName_orig = lvItem.VolumeName;
                String strVolumeName = null;
                String strSaveAs = lvItem.SaveAs;

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

                String strDriveLetter_orig = null;
                String strDriveLetter = null;

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
                    String strLine = null;
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
            if (Utilities.ValidateFile(CBSaveAs.S) == false)
            {
                return false;
            }

            using (StreamReader file = new StreamReader(CBSaveAs.S))
            {
                String line = null;

                if ((line = file.ReadLine()) == null) return false;
                if ((line = file.ReadLine()) == null) return false;
                if (line.StartsWith(Utilities.mSTRlineType_Nickname) == false) return false;

                String[] arrLine = line.Split('\t');
                String strName = String.Empty;

                if (arrLine.Length > 2) strName = arrLine[2];
                CBVolumeName.S = strName;
                if ((line = file.ReadLine()) == null) return false;
                if (line.StartsWith(Utilities.mSTRlineType_Path) == false) return false;
                arrLine = line.Split('\t');
                if (arrLine.Length < 3) return false;
                CBPath.S = arrLine[2];
            }

            return SaveFields(false);
        }

        void RemoveVolume()
        {
            VolumeLVitemVM[] lvSelect = m_app.xaml_lvVolumesMain.SelectedItems.Cast<VolumeLVitemVM>().ToArray();

            if (lvSelect.Length <= 0)
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

                foreach (VolumeLVitemVM lvItem in m_VolsLVvm.Items)
                {
                    if (lvItem.treeNode == null)
                    {
                        // scenario: unsaved file
                        continue;
                    }

                    RootNodeDatum rootNodeDatum = (RootNodeDatum)lvItem.treeNode.Tag;

                    if (lvSelect.Contains(lvItem))
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

                foreach (VolumeLVitemVM lvItem in lvSelect)
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

            foreach (VolumeLVitemVM lvItem in lvSelect)
            {
                m_VolsLVvm.Items.Remove(lvItem);
            }
        }

        void SaveAs()
        {
            SDL_File.Init();
            SDL_File.SFD.Filter = SDL_File.FileAndDirListFileFilter + "|" + SDL_File.BaseFilter;

            if (Utilities.StrValid(CBSaveAs.S))
            {
                SDL_File.SFD.InitialDirectory = Path.GetDirectoryName(CBSaveAs.S);
            }

            if (SDL_File.SFD.ShowDialog() == Forms.DialogResult.OK)
            {
                CBSaveAs.S = SDL_File.SFD.FileName;

                if (File.Exists(CBSaveAs.S))
                {
                    CBVolumeName.S = null;
                    CBPath.S = null;
                }
            }
        }

        void SaveDirLists()
        {
            bool bRestartTreeTimer = gd.timer_DoTree.IsEnabled;

            gd.timer_DoTree.Stop();

 //           if ((DoSaveDirListings() == false) && bRestartTreeTimer)   // cancelled
            {
                gd.RestartTreeTimer();
            }
        }

        bool SaveFields(bool bFailOnDirectory = true)
        {
            CBVolumeName.S = Utilities.NotNull(m_app.xaml_cbVolumeName.Text).Trim();
            CBPath.S = Utilities.NotNull(m_app.xaml_cbPath.Text).Trim();

            if (Utilities.StrValid(CBPath.S))
            {
                CBPath.S += Path.DirectorySeparatorChar;

                String str = FormatPath(CBPath.S, m_app.xaml_cbPath, bFailOnDirectory);

                if (str != null)
                {
                    CBPath.S = str;
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
                    CBSaveAs.S = Path.GetFullPath(m_app.xaml_cbSaveAs.Text.Trim());
                }
                catch
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Error in save listings filename.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(Path.GetDirectoryName(CBSaveAs.S)) == false)
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Directory to save listings to doesn't exist.", "Save Fields");
                    return false;
                }

                if (Directory.Exists(CBSaveAs.S))
                {
                    gd.FormError(m_app.xaml_cbSaveAs, "Must specify save filename. Only directory entered.", "Save Fields");
                    return false;
                }

                String str = FormatPath(CBSaveAs.S, m_app.xaml_cbSaveAs, bFailOnDirectory);

                if (str != null)
                {
                    CBSaveAs.S = str;
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
            if (HasItems)
            {
                new SDL_VolumeFile().WriteList(m_VolsLVvm.Items);
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
                CBPath.S = folderBrowserDialog1.SelectedPath;
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

                SortedDictionary<String, object> dictVolGroups = new SortedDictionary<String, object>();

                foreach (VolumeLVitemVM lvItem in m_VolsLVvm.Items)
                {
                    if ((lvItem.VolumeGroup != null) && (dictVolGroups.ContainsKey(lvItem.VolumeGroup) == false))
                    {
                        dictVolGroups.Add(lvItem.VolumeGroup, null);
                    }
                }

                foreach (KeyValuePair<String, object> entry in dictVolGroups)
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
