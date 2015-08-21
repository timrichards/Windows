using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FilesVM : IDisposable
    {
        internal LV_FilesVM()
        {
            _lsDisposable.Add(TreeSelect.FileListUpdated.Observable.LocalSubscribe(99703, TreeSelect_FileListUpdated));
            _lsDisposable.Add(WinTreeMapVM.SelectedFile.LocalSubscribe(99702, UC_TreeMap_SelectedFile));

            var fileList = LocalTV.TreeSelect_FileList;

            if (null != fileList)
                TreeSelect_FileListUpdated(Tuple.Create(fileList, 0));

            _wr.SetTarget(this);
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        void TreeSelect_FileListUpdated(Tuple<TreeSelect.FileListUpdated, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("J");
            if (tuple.treeNode == _treeNode)
                return;

            SelectedItem_Set(null, initiatorTuple.Item2);
            ClearItems();
            _treeNode = tuple.treeNode;

            if (null == tuple.ieFiles)
                return;

            var lsItems = new List<LVitem_FilesVM>();

            foreach (var strFileLine in tuple.ieFiles)
            {
                var asFileLine =
                    strFileLine
                    .Split('\t')
                    .ToArray();

                var nLine = ("" + asFileLine[1]).ToInt();
                var lsDuplicates = Statics.FileDictionary.GetDuplicates(asFileLine);

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
                            (dupe.LVitemProjectVM.ListingFile != tuple.strListingFile) ||    // exactly once every query
                            (dupe.LineNumber != nLine));

                    lvItem.SameVolume =
                        lsDuplicates
                        .GroupBy(duplicate => duplicate.LVitemProjectVM.Volume)
                        .HasExactly(1);
                }

                lsItems.Add(lvItem);
            }

            Util.UIthread(99813, () => Add(lsItems));

            if (null != tuple.strFilename)
            {
                this[tuple.strFilename].FirstOnlyAssert(fileVM =>
                    SelectedItem_Set(fileVM, initiatorTuple.Item2));
            }
        }

        void UC_TreeMap_SelectedFile(Tuple<string, int> initiatorTuple)
        {
            Util.Write("B");

            this[initiatorTuple.Item1].FirstOnlyAssert(fileVM =>
                SelectedItem_Set(fileVM, initiatorTuple.Item2));
        }

        internal override IEnumerable<LVitem_FilesVM> this[string s_in]
        {
            get
            {
                if (null == s_in)
                    return null;
            
                var s = s_in.ToLower();

                return ItemsCast.Where(o => ("" + o.Filename).ToLower().Equals(s));
            }
        }

        LocalTreeNode
            _treeNode = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
        static readonly WeakReference<LV_FilesVM>
            _wr = new WeakReference<LV_FilesVM>(null);
    }
}
