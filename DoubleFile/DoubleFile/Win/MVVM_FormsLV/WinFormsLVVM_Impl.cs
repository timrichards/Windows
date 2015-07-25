using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class WinFormsLVVM
    {
        internal WinFormsLVVM(string strFragment)
        {
            LocalLVVM localLVVM = null;

            switch (strFragment)
            {
                case WinFormsLV.FolderListSolitary:
                {
                    Util.WriteLine("FolderListUnique");
                    localLVVM = LocalTV.Solitary;
                    break;
                }

                case WinFormsLV.FolderListSameVol:
                {
                    Util.WriteLine("FolderListSameVol");
                    localLVVM = LocalTV.SameVol;
                    break;
                }

                case WinFormsLV.FolderListClones:
                {
                    Util.WriteLine("FolderListClones");
                    localLVVM = LocalTV.Clones;
                    break;
                }

                default:
                {
                    Util.Assert(99784, false);
                    return;
                }
            }

            if (localLVVM.Items.Any())
                Add(localLVVM.ItemsCast);

            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));
        }

        internal WinFormsLVVM Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            return this;
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposable);
        }

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            ItemsCast
                .Where(lvItem => lvItem.WithLocalTreeNode(t => t) == tuple.treeNode)
                .FirstOnlyAssert(SelectedItem_Set);
        }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99783, false);    // binding should dim the button
                return;
            }

            _selectedItem.WithLocalTreeNode(t => t
                .GoToFile(null));
        }

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
