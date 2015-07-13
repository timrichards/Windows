using System.Linq;
using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinFolderListVM
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
                    return bAlt = !bAlt;
                };

                Util.UIthread(99828, () => Add(LocalTV.AllNodes
                    .OrderByDescending(folder => folder.NodeDatum.FolderScore[nFolderScoreIndex])
                    .Select(folder => new LVitem_FolderListVM { LocalTreeNode = folder, Alternate = Alternate(folder) })));
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

                case WinFolderList.FolderListUnique:
                {
                    Util.WriteLine("FolderListUnique");
                    break;
                }

                case WinFolderList.FolderListSameVol:
                {
                    Util.WriteLine("FolderListSameVol");
                    break;
                }

                case WinFolderList.FolderListClones:
                {
                    Util.WriteLine("FolderListClones");
                    break;
                }

                default:
                {
                    MBoxStatic.Assert(99887, false);
                    break;
                }
            }

            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Subscribe(TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));
        }

        internal WinFolderListVM Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            return this;
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposable);
        }

        void TreeSelect_FolderDetailUpdated(Tuple<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == tuple.Item2)
                .FirstOnlyAssert(SelectedItem_Set);
        }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                MBoxStatic.Assert(99897, false);    // binding should dim the button
                return;
            }

            _selectedItem.LocalTreeNode.GoToFile(null);
        }

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
