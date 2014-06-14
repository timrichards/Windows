using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;

namespace SearchDirLists
{
    partial class BrowseTabVM
    {
        #region Navbar
        void Collapse() { }
        void CompareCheck() { }
        void Compare() { }
        void Up() { }
        void Back() { }
        void Forward() { }
        void Copy() { }
        void SearchFolders() { }
        void SearchFoldersAndFiles() { }
        void SearchFiles() { }
        #endregion Navbar

        #region Copy Scratchpad
        void CopyScratchpad_Script() { }
        void CopyScratchpad_Load() { }
        void CopyScratchpad_Save() { }
        void CopyScratchpad_Clear() { }
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

        void Ignore_Delete() { }

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

        void Ignore_Save() { }
        void Ignore_Clear() { }
        #endregion Ignore List
    }
}
