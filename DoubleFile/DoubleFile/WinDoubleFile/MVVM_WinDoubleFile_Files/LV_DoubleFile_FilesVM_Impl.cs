using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_DoubleFile_FilesVM : IDisposable
    {
        internal LV_DoubleFile_FilesVM()
        {
            Local.TreeSelect.FileListUpdated += TreeSelect_FileList;
            LocalTreeNode.SelectedFile += SelectedFile;
            TreeViewItem_DoubleFileVM.SelectedFile += SelectedFile;
        }

        public void Dispose()
        {
            Local.TreeSelect.FileListUpdated -= TreeSelect_FileList;
            LocalTreeNode.SelectedFile -= SelectedFile;
            TreeViewItem_DoubleFileVM.SelectedFile -= SelectedFile;
        }

        void TreeSelect_FileList(IEnumerable<string> lsFileLines, string strListingFile)
        {
            _selectedItem = null;
            UtilProject.UIthread(Items.Clear);

            if (null == lsFileLines)
                return;

            var lsItems = new List<LVitem_DoubleFile_FilesVM>();

            foreach (var strFileLine in lsFileLines)
            {
                string strFilename = null;
                var nLine = -1;
                var lsDuplicates = App.FileDictionary.GetDuplicates(strFileLine, out strFilename, out nLine);
                var nCount = (null != lsDuplicates) ? lsDuplicates.Count() - 1 : 0;
                var strCount = (nCount > 0) ? "" + nCount : null;
                var lvItem = new LVitem_DoubleFile_FilesVM(new[] { strFilename, strCount });

                lvItem.FileLine = strFileLine;

                if (0 < nCount)
                {
                    lvItem.LSduplicates =
                        lsDuplicates
                        .Where(dupe =>
                            (dupe.LVitemProjectVM.ListingFile != strListingFile) ||    // exactly once every query
                            (dupe.LineNumber != nLine)
                        );

                    lvItem.SameVolume =
                        (1 ==
                        lsDuplicates
                            .GroupBy(duplicate => duplicate.LVitemProjectVM.Volume)
                            .Count());
                }
                else    // solitary file
                {
                    var asFile = strFileLine.Split('\t');

                    lvItem.SolitaryAndNonEmpty =
                        (asFile.Length <= FileParse.knColLength) ||                       // doesn't happen
                        string.IsNullOrWhiteSpace(asFile[FileParse.knColLength]) ||       // doesn't happen
                        ulong.Parse(asFile[FileParse.knColLength]) > 0;
                }

                lsItems.Add(lvItem);
            }

            UtilProject.UIthread(() => Add(lsItems));

            SelectedItem_Set(this[_strSelectedFile]);
            _strSelectedFile = null;
        }

        void SelectedFile(string strFile)
        {
            _strSelectedFile = strFile;
            SelectedItem_Set(this[_strSelectedFile]);
        }

        string _strSelectedFile = null;
    }
}
