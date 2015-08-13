using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class WinClonesVM : IDisposable
    {
        // The create/dispose model is different for this class than anywhere else.
        // The class is created 3x in LocalTV using this empty constructor, and held there statically.
        // WinClones.xaml.cs will call upon FactoryGetHolder on each LocalNavigatedTo(), and then
        // calls Dispose() on the static object, UX-repeating this cycle without ever destroying
        // the three objects created by this default constructor.
        internal WinClonesVM()
        {
        }

        internal static WinClonesVM FactoryGetHolder(string strFragment)
        {
            WinClonesVM localLVVM = null;

            switch (strFragment)
            {
                case WinClones.FolderListSolitary:
                {
                    Util.WriteLine("FolderListUnique");
                    localLVVM = LocalTV.Solitary;
                    break;
                }

                case WinClones.FolderListSameVol:
                {
                    Util.WriteLine("FolderListSameVol");
                    localLVVM = LocalTV.SameVol;
                    break;
                }

                case WinClones.FolderListClones:
                {
                    Util.WriteLine("FolderListClones");
                    localLVVM = LocalTV.Clones;
                    break;
                }

                default:
                {
                    Util.Assert(99784, false);
                    return null;
                }
            }

            return
                localLVVM
                .Init();
        }

        WinClonesVM Init()
        {
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99700, TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));

            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);

            _nicknameUpdater = ItemsCast.Skip(1 /* marker */).Select(lvItem => lvItem.NicknameUpdater).FirstOrDefault();

            if (null != _nicknameUpdater)
            {
                Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
                _nicknameUpdater.Clear();
                _nicknameUpdater.UpdateViewport(UseNicknames);
            }
            else
            {
                Util.Assert(99865, false, bTraceOnly: true);
            }

            return this;
        }

        public void Dispose()
        {
            _nicknameUpdater.Clear();
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

        ListUpdater<bool>
            _nicknameUpdater = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
