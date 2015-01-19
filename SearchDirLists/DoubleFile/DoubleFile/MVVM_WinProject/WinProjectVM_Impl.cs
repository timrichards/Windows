﻿using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinProjectVM
    {
        // Menu items
        
        static string ksAllFilesFilter = "|All files|*.*";
        static string ksProjectFilter = "Double File project|*." + FileParse.ksFileExt_Project + ksAllFilesFilter;
        internal static string ksListingFilter = "Double File Listing|*." + FileParse.ksFileExt_Listing + ksAllFilesFilter;

        internal void OpenProject()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Open Project";
            dlg.Filter = ksProjectFilter;

            if (dlg.ShowDialog() ?? false)
            {
                new ProjectFile().OpenProject(dlg.FileName, OpenListingFiles);
            }
        }

        internal void SaveProject()
        {
            SaveProject(m_lvVM.ItemsCast);
        }

        static internal void SaveProject(IEnumerable<LVitem_ProjectVM> list_lvVolStrings)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.Title = "Save Project";
            dlg.Filter = ksProjectFilter;

            if (dlg.ShowDialog() ?? false)
            {
                new ProjectFile().SaveProject(list_lvVolStrings, dlg.FileName);
            }
        }

        internal void NewListingFile()
        {
            var lvItemVolumeTemp = new LVitem_ProjectVM(new string[] { });

            while (true)
            {
                var newVolume = new WinVolumeNew();

                newVolume.LVitemVolumeTemp = new LVitem_ProjectVM(lvItemVolumeTemp);

                if ((newVolume.ShowDialog() ?? false) == false)
                {
                    // user cancelled
                    break;
                }

                if ((false == m_lvVM.AlreadyInProject(null, newVolume.LVitemVolumeTemp.ListingFile)) &&
                    (false == m_lvVM.FileExists(newVolume.LVitemVolumeTemp.ListingFile)))
                {
                    m_lvVM.NewItem(newVolume.LVitemVolumeTemp);
                    break;
                }

                lvItemVolumeTemp = new LVitem_ProjectVM(newVolume.LVitemVolumeTemp);
            }
        }

        internal void OpenListingFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Open Listing File";
            dlg.Filter = ksListingFilter;
            dlg.Multiselect = true;

            if (dlg.ShowDialog() ?? false)
            {
                OpenListingFiles(dlg.FileNames);
            }
        }

        internal void OpenListingFiles(IEnumerable<string> listFiles, bool bClearItems = false)
        {
            LVitem_ProjectVM lvItem = null;
            var sbBadFiles = new System.Text.StringBuilder();
            bool bMultiBad = true;

            if (bClearItems)
            {
                m_lvVM.Items.Clear();
            }

            foreach (var fileName in listFiles)
            {
                if (FileParse.ReadHeader(fileName, out lvItem))
                {
                    m_lvVM.NewItem(lvItem);
                }
                else
                {
                    bMultiBad = (sbBadFiles.Length > 0);
                    sbBadFiles.Append("• ").Append(System.IO.Path.GetFileName(fileName)).Append("\n");
                }
            }

            if (sbBadFiles.Length > 0)
            {
                MBox.ShowDialog("Bad listing file" + (bMultiBad ? "s" : "") + ".\n" + sbBadFiles, "Open Listing File");
            }
        }
    }
}