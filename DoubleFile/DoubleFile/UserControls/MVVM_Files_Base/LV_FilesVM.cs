using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FilesVM : LV_FilesVM_Base, IDisposable
    {
        internal LV_FilesVM()
        {
            _wr.SetTarget(this);
            _lsDisposable.Add(TreeSelect.FileListUpdated.Observable.LocalSubscribe(99703, TreeSelect_FileListUpdated));
            _lsDisposable.Add(UC_TreemapVM.SelectedFile.LocalSubscribe(99702, UC_Treemap_SelectedFile));

            var fileList = LocalTV.TreeSelect_FileList;

            if (null != fileList)
                TreeSelect_FileListUpdated(Tuple.Create(fileList, /* UI initiator */ 0m));
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        void TreeSelect_FileListUpdated(Tuple<TreeSelect.FileListUpdated, decimal> initiatorTuple)
        {
            var fileList = initiatorTuple.Item1;

            Util.Write("J");

            if (fileList.treeNode != _treeNode)
                TreeSelect_FileListUpdated_(initiatorTuple);

            if (null != fileList.strFilename)
            {
                this[fileList.strFilename].FirstOnlyAssert(fileVM =>
                    SelectedItem_Set(fileVM, initiatorTuple.Item2));
            }
        }

        void TreeSelect_FileListUpdated_(Tuple<TreeSelect.FileListUpdated, decimal> initiatorTuple)
        {
            var fileList = initiatorTuple.Item1;

            SelectedItem_Set(null, initiatorTuple.Item2);
            ClearItems();
            _treeNode = fileList.treeNode;

            if (null == fileList.ieFiles)
                return;

            var lsItems = new List<LVitem_FilesVM>();
            var strListingFile = fileList.treeNode.RootNodeDatum.LVitemProjectVM.ListingFile;

            foreach (var strFileLine in fileList.ieFiles)
            {
                var asFileLine =
                    strFileLine
                    .Split('\t')
                    .ToArray();

                var nLine = ("" + asFileLine[1]).ToInt();
                var lsDuplicates = Statics.DupeFileDictionary.GetDuplicates(asFileLine);

                asFileLine =
                    asFileLine
                    .Skip(3)                            // makes this an LV line: knColLengthLV
                    .ToArray();

                var nDuplicates = (null != lsDuplicates) ? lsDuplicates.Count - 1 : 0;
                var lvItem = new LVitem_FilesVM { DuplicatesRaw = nDuplicates, FileLine = asFileLine };

                if (0 < nDuplicates)
                {
                    lvItem.LSduplicates =
                        lsDuplicates
                        .Where(dupe =>
                            (dupe.LVitemProjectVM.ListingFile != strListingFile) ||    // exactly once every query
                            (dupe.LineNumber != nLine));

                    lvItem.SameVolume =
                        lsDuplicates
                        .GroupBy(duplicate => duplicate.LVitemProjectVM.Volume)
                        .HasExactly(1);
                }

                lsItems.Add(lvItem);
            }

            if (0 < lsItems.Count)
                Util.UIthread(99813, () => Add(lsItems));
        }

        void UC_Treemap_SelectedFile(Tuple<string, decimal> initiatorTuple)
        {
            Util.Write("B");

            this[initiatorTuple.Item1].FirstOnlyAssert(fileVM =>
                SelectedItem_Set(fileVM, initiatorTuple.Item2));
        }

        LocalTreeNode
            _treeNode = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
