using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_DoubleFile_FilesVM : IDisposable
    {
        internal LV_DoubleFile_FilesVM()
        {
            _lsDisposable.Add(TreeSelect.FileListUpdated.Subscribe(TreeSelect_FileListUpdated));
            _lsDisposable.Add(LocalTreeNode.SelectedFile.Subscribe(LocalTreeNode_SelectedFile));
            _lsDisposable.Add(UC_TreeMap.SelectedFile.Subscribe(UC_TreeMap_SelectedFile));
        }

        public void Dispose()
        {
            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        void TreeSelect_FileListUpdated(Tuple<Tuple<IEnumerable<string>, string, LocalTreeNode>, int> tupleA)
        {
            var tuple = tupleA.Item1;

            UtilDirList.Write("J");
            if (tuple.Item3 == _treeNode)
                return;

            SelectedItem_Set(null, tupleA.Item2);
            UtilProject.UIthread(ClearItems);
            _treeNode = tuple.Item3;

            if (null == tuple.Item1)
                return;

            var lsItems = new List<LVitem_DoubleFile_FilesVM>();

            foreach (var strFileLine in tuple.Item1)
            {
                var asFileLine =
                    strFileLine
                    .Split('\t')
                    .ToArray();

                var nLine = int.Parse(asFileLine[1]);
                var lsDuplicates = App.FileDictionary.GetDuplicates(asFileLine);

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
                            (dupe.LVitemProjectVM.ListingFile != tuple.Item2) ||    // exactly once every query
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

            if (null != _strSelectedFile)
                SelectedItem_Set(this[_strSelectedFile].FirstOrDefault(), tupleA.Item2);

            _strSelectedFile = null;
        }

        void LocalTreeNode_SelectedFile(Tuple<string, int> tupleA)
        {
            UtilDirList.Write("A");
            SelectedFile(tupleA);
        }

        void UC_TreeMap_SelectedFile(Tuple<string, int> tupleA)
        {
            UtilDirList.Write("B");
            SelectedFile(tupleA);
        }

        void SelectedFile(Tuple<string, int> tupleA)
        {
            SelectedItem_Set(this[tupleA.Item1].FirstOrDefault(), tupleA.Item2);
        }

        internal override IEnumerable<LVitem_DoubleFile_FilesVM> this[string s_in]
        {
            get
            {
                if (null == s_in)
                    return null;
            
                var s = s_in.ToLower();

                return ItemsCast.Where(o => o.Filename.ToLower().Equals(s));
            }
        }

        string
            _strSelectedFile = null;
        LocalTreeNode
            _treeNode = null;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
