using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class UC_ClonesVM
    {
        internal class VMPackage : IDisposable
        {
            internal ListUpdater<bool>
                NicknameUpdater;
            internal readonly IList<IDisposable>
                Disposables = new List<IDisposable> { };

            public void Dispose()
            {
                NicknameUpdater?.Clear();
                Util.LocalDispose(Disposables);
            }
        }

        // The create/dispose model is different for this class than anywhere else.
        // The class is created 3x in LocalTV using this empty constructor, and held there statically.
        // WinClones.xaml.cs will call upon FactoryGetHolder on each LocalNavigatedTo(), and then
        // calls Dispose() on the static object, UX-repeating this cycle without ever destroying
        // the three objects created by this default constructor.
        internal UC_ClonesVM()
        {
        }

        static internal UC_ClonesVM
            FactoryGetHolder(string strFragment, VMPackage vmPackage)
        {
            UC_ClonesVM localLVVM = null;

            switch (strFragment)
            {
                case UC_Clones.FolderListSolitary:
                {
                    Util.WriteLine("FolderListUnique");
                    localLVVM = LocalTV.LVsolitary;
                    break;
                }

                case UC_Clones.FolderListSameVol:
                {
                    Util.WriteLine("FolderListSameVol");
                    localLVVM = LocalTV.LVsameVol;
                    break;
                }

                case UC_Clones.FolderListClones:
                {
                    Util.WriteLine("FolderListClones");
                    localLVVM = LocalTV.LVclones;
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
                .Init(vmPackage);
        }

        UC_ClonesVM Init(VMPackage vmPackage)
        {
            vmPackage.Disposables.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99700, TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, /* UI initiator */ 0m));

            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);

            vmPackage.NicknameUpdater = ItemsCast.Skip(1 /* marker */).Select(lvItem => lvItem.NicknameUpdater).FirstOrDefault();

            if (null != vmPackage.NicknameUpdater)
            {
                Icmd_Nicknames = new RelayCommand(() => vmPackage.NicknameUpdater.UpdateViewport(UseNicknames));
                vmPackage.NicknameUpdater.Clear();
                vmPackage.NicknameUpdater.UpdateViewport(UseNicknames);
            }
            else
            {
                Util.Assert(99865, 0 == Items.Count, bIfDefDebug: true);
            }

            RaisePropertyChanged("UseNicknames");
            return this;
        }

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, decimal> initiatorTuple)
        {
            var folderDetail = initiatorTuple.Item1;

            ItemsCast
                .Where(lvItem =>
            {
                foreach (var wr in lvItem.TreeNodes)
                {
                    if (ReferenceEquals(wr.Get(w => w), folderDetail.treeNode))
                        return true;
                }

                return false;
            })
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
    }
}
