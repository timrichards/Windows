using System.Linq;
using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinFolderListVM : IDisposable
    {
        internal WinFolderListVM(string strFragment)
        {
            var nPrev = uint.MaxValue;
            var bAlt = false;

            Action<int> AddFolders = nFolderScoreIndex =>
            {
                Func<LocalTreeNode, bool> Alternate = folder =>
                {
                    var nFolderScore = folder.NodeDatum.FolderScore[nFolderScoreIndex];

                    if (nPrev == nFolderScore)
                        return bAlt;

                    nPrev = nFolderScore;
                    return bAlt = (false == bAlt);
                };

                Util.UIthread(99828, () => Add(LocalTV.AllNodes
                    .OrderByDescending(folder => folder.NodeDatum.FolderScore[nFolderScoreIndex])
                    .Select(folder => new LVitem_FolderListVM(folder, Alternate(folder), _nicknameUpdater))));
            };

            switch (strFragment)
            {
                case WinFolderList.FolderListLarge:
                {
                    AddFolders(1);
                    Util.WriteLine("FolderListLarge");
                    break;
                }

                case WinFolderList.FolderListSmall:
                {
                    AddFolders(2);
                    Util.WriteLine("FolderListSmall");
                    break;
                }

                case WinFolderList.FolderListRandom:
                {
                    AddFolders(0);
                    Util.WriteLine("FolderListRandom");
                    break;
                }

                default:
                {
                    Util.Assert(99887, false);
                    return;
                }
            }

            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99701, TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));
        }

        internal WinFolderListVM Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateNicknames(UseNicknames));
            _nicknameUpdater.UseNickname = UseNicknames;
            _nicknameUpdater.Clear();
            return this;
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == tuple.treeNode)
                .FirstOnlyAssert(SelectedItem_Set);
        }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99897, false);    // binding should dim the button
                return;
            }

            _selectedItem.LocalTreeNode.GoToFile(null);
        }

        NicknameUpdater
            _nicknameUpdater = new NicknameUpdater { };
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
