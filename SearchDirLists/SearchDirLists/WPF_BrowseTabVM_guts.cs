using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using System.Windows.Controls;

namespace SearchDirLists
{
    partial class BrowseTabVM
    {
        #region Navbar
        void Collapse() { }
        void CompareCheck() { }
        void Compare() { }
        void Up() { }
        void Back()
        {
            gd.DoHistory(m_app.xaml_btnBack, -1);
        }

        void Forward()
        {
            gd.DoHistory(m_app.xaml_btnForward, +1);
        }

        void Copy() { }
        void SearchFolders() { Search(m_app.xaml_btnFolders); }
        void SearchFoldersAndFiles() { Search(m_app.xaml_btnFoldersFiles); }
        void SearchFiles() { Search(m_app.xaml_btnFiles); }

        void Search(Control ctl)
        {
            //gd.m_strSelectFile = null;

            //if (Utilities.NotNull(m_app.xaml_cbFindbox.Text).Trim().Length > 0)
            //{
            //    if (m_ctlLastSearchSender != sender)
            //    {
            //        m_ctlLastSearchSender = (SDL_Control)sender;
            //        gd.m_nSearchResultsIndexer = -1;
            //    }

            //    if ((gd.m_nSearchResultsIndexer < 0) && new Button[] { form_btnFoldersAndFiles, form_btnFiles }.Contains(sender))
            //    {
            //        DoSearchType2(form_cbFindbox.Text, bSearchFilesOnly: (sender == form_btnFiles));
            //    }
            //    else
            //    {
            //        DoSearch(sender);
            //    }
            //}
            //else
            //{
            //    gd.m_blinky.Go(clr: Color.Red, Once: true);
            //}
        }
        #endregion Navbar

        #region Copy Scratchpad
        void CopyScratchpad_Script() { }

        void CopyScratchpad_Load()
        {
            //ListView lvFake = new ListView();   // Hack: check changed event loads the real listviewer

            //foreach (ColumnHeader col in form_lvCopyScratchpad.Columns)
            //{
            //    lvFake.Columns.Add(new ColumnHeader());
            //}

            //if (new SDL_CopyFile().ReadList(lvFake) == false)
            //{
            //    return;
            //}

            //LoadCopyScratchPad(lvFake);
        }

        void CopyScratchpad_Save()
        {
            if (LV_CopyScratchpad.HasItems)
            {
                new SDL_CopyFile().WriteList(LV_CopyScratchpad.ItemsCast);
            }
            else
            {
                gd.m_blinky.Go(ctl: m_app.xaml_btnCopyScratchpadSave, clr: Drawing.Color.Red, Once: true);
            }
        }

        void CopyScratchpad_Clear()
        {
            //foreach (SDL_ListViewItem lvItem in LV_CopyScratchpad.ItemsCast)
            //{
            //    if (lvItem.treeNode != null)
            //    {
            //        lvItem.treeNode.Checked = false;
            //    }
            //    else
            //    {
            //        lvItem.Remove();    // 1. sorted by size. 2. ClearMem_TreeForm() does null lvItems.
            //    }
            //}
        }
        #endregion Copy Scratchpad

        #region Ignore List
        void Ignore_Loose() { }

        void Ignore_Add()
        {
            SDL_TreeNode treeNode = (SDL_TreeNode)SDLWPF.treeViewMain.SelectedNode;

            if (treeNode == null)
            {
                gd.m_blinky.Go(m_app.xaml_btnIgnoreAdd, clr: Drawing.Color.Red, Once: true);
            }
            else if (LV_Ignore.Contains(treeNode.Text))
            {
        //        gd.m_blinky.SelectLVitem(lvItem: LV_Ignore[treeNode.Text]);
            }
            else
            {
                new IgnoreLVitemVM(LV_Ignore, new String[] { treeNode.Text, (treeNode.Level + 1).ToString() });
                gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
                gd.RestartTreeTimer();
            }
        }

        void Ignore_Delete()
        {
            if (LV_Ignore.HasItems == false)
            {
                gd.m_blinky.Go(m_app.xaml_btnIgnoreDel, clr: Drawing.Color.Red, Once: true);
                return;
            }

            foreach (IgnoreLVitemVM lvItem in LV_Ignore.Selected.ToList())
            {
                LV_Ignore.Items.Remove(lvItem);
            }

            gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
            gd.RestartTreeTimer();
        }

        void Ignore_Load()
        {
            if (new SDL_IgnoreFile().ReadList(LV_Ignore) == false)
            {
                return;
            }

            if (LV_Ignore.HasItems)
            {
                gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
                gd.RestartTreeTimer();
            }
        }

        void Ignore_Save()
        {
            if (LV_Ignore.HasItems)
            {
                new SDL_IgnoreFile().WriteList(LV_Ignore.ItemsCast);
            }
            else
            {
                gd.m_blinky.Go(ctl: m_app.xaml_btnIgnoreSave, clr: Drawing.Color.Red, Once: true);
            }
        }

        void Ignore_Clear()
        {
            if (LV_Ignore.HasItems == false)
            {
                return;
            }

            LV_Ignore.Items.Clear();
            gd.m_bKillTree &= gd.timer_DoTree.IsEnabled;
            gd.RestartTreeTimer();
        }
        #endregion Ignore List
    }
}
