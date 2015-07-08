using System.Linq;
using System;

namespace DoubleFile
{
    partial class WinFolderListVM
    {
        internal WinFolderListVM Init(string strFragment)
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);

            var nPrev = uint.MaxValue;
            var bAlt = false;

            Action<int> AddFolders = (nFolderScoreIndex) =>
            {
                Func<LocalTreeNode, bool> Alternate = folder =>
                {
                    var nFolderScore = folder.NodeDatum.FolderScore[nFolderScoreIndex];

                    if (nPrev == nFolderScore)
                        return bAlt;

                    nPrev = nFolderScore;
                    return bAlt = !bAlt;
                };

                Util.UIthread(() => Add(LocalTV.AllNodes
                    .OrderByDescending(folder => folder.NodeDatum.FolderScore[nFolderScoreIndex])
                    .Select(folder => new LVitem_FolderListVM { LocalTreeNode = folder, Alternate = Alternate(folder) })));
            };

            switch (strFragment)
            {
                case MainWindow.FolderListLarge:
                {
                    AddFolders(1);
                    Util.WriteLine("FolderListLarge");
                    break;
                }

                case MainWindow.FolderListSmall:
                {
                    AddFolders(2);
                    Util.WriteLine("FolderListSmall");
                    break;
                }

                case MainWindow.FolderListRandom:
                {
                    AddFolders(0);
                    Util.WriteLine("FolderListRandom");
                    break;
                }

                case MainWindow.FolderListUnique:
                {
                    Util.WriteLine("FolderListUnique");
                    break;
                }

                case MainWindow.FolderListSameVol:
                {
                    Util.WriteLine("FolderListSameVol");
                    break;
                }

                case MainWindow.FolderListClones:
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

            return this;
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
    }
}
