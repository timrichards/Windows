using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_DoubleFile_FilesVM : IDisposable
    {
        internal LV_DoubleFile_FilesVM()
        {
            TreeSelect.FileListUpdated += TreeSelect_FileList;
            LocalTreeNode.SelectedFile += SelectedFile;
            UC_TreeMap.SelectedFile += SelectedFile;
        }

        public void Dispose()
        {
            TreeSelect.FileListUpdated -= TreeSelect_FileList;
            LocalTreeNode.SelectedFile -= SelectedFile;
            UC_TreeMap.SelectedFile -= SelectedFile;
        }

        void TreeSelect_FileList(IEnumerable<string> lsFileLines, string strListingFile, LocalTreeNode treeNode)
        {
            if (treeNode == _treeNode)
            {
                return;
            }

            SelectedItem_Set(null);
            UtilProject.UIthread(Items.Clear);
            _treeNode = treeNode;

            if (null == lsFileLines)
                return;

            var lsItems = new List<LVitem_DoubleFile_FilesVM>();

            foreach (var strFileLine in lsFileLines)
            {
                var asFileLine =
                    strFileLine
                    .Split('\t')
                    .ToArray();

                var nLine = int.Parse(asFileLine[1]);
                var lsDuplicates = MainWindow.FileDictionary.GetDuplicates(asFileLine);

                asFileLine =
                    asFileLine
                    .Skip(3)                            // makes this an LV line: knColLengthLV
                    .ToArray();

                var nDuplicates = (null != lsDuplicates) ? lsDuplicates.Count() - 1 : 0;
                var lvItem = new LVitem_DoubleFile_FilesVM() { Duplicates_ = nDuplicates, FileLine = asFileLine };

                if (0 < nDuplicates)
                {
                    lvItem.LSduplicates =
                        lsDuplicates
                        .Where(dupe =>
                            (dupe.LVitemProjectVM.ListingFile != strListingFile) ||    // exactly once every query
                            (dupe.LineNumber != nLine));

                    lvItem.SameVolume =
                        (1 ==
                        lsDuplicates
                            .GroupBy(duplicate => duplicate.LVitemProjectVM.Volume)
                            .Count());
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

        internal override LVitem_DoubleFile_FilesVM this[string s_in]
        {
            get
            {
                if (null == s_in)
                    return null;
            
                var s = s_in.ToLower();

                return ItemsCast.FirstOrDefault(o => o.Filename.ToLower().Equals(s));
            }
        }

        string
            _strSelectedFile = null;
        LocalTreeNode
            _treeNode = null;
    }
}
