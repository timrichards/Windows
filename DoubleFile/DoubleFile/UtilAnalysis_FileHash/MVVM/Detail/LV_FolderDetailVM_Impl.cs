﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class LV_FolderDetailVM : IDisposable
    {
        internal LV_FolderDetailVM(GlobalData_Base gd)
        {
            _gd = gd;
            Local.TreeSelect.FolderDetailUpdated += TreeSelect_FolderDetail;
        }

        public void Dispose()
        {
            Local.TreeSelect.FolderDetailUpdated -= TreeSelect_FolderDetail;
        }

        void TreeSelect_FolderDetail(IEnumerable<string[]> lasDetail, LocalTreeNode treeNode)
        {
            UtilProject.UIthread(() =>
            {
                Title = null;
                Items.Clear();

                foreach (var asLine in lasDetail)
                    Add(new LVitem_FolderDetailVM(asLine), bQuiet: true);

                var strFG_Description = UtilColor.Description[treeNode.ForeColor];
                var strBG_Description = UtilColor.Description[treeNode.BackColor];

                if (false == string.IsNullOrEmpty(strFG_Description))
                    Add(new LVitem_FolderDetailVM(new[] { "", strFG_Description }), bQuiet: true);

                if (false == string.IsNullOrEmpty(strBG_Description))
                    Add(new LVitem_FolderDetailVM(new[] { "", strBG_Description }), bQuiet: true);

                Add(new LVitem_FolderDetailVM(), bQuiet: true);
                Title = treeNode.Text;
                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}