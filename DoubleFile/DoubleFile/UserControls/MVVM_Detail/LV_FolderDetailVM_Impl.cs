using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FolderDetailVM : IDisposable
    {
        internal LV_FolderDetailVM()
        {
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99706, TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));
        }

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("G");
            Title = null;
            ClearItems();

            if (null == tuple.treeNode)
                return;

            Util.UIthread(99818, () =>
            {
                var ieDetail =
                    tuple.ieDetail
                    .Select(ieLine => new LVitem_FolderDetailVM(ieLine.ToList()));

                var strFG_Description = UtilColorcode.Descriptions[tuple.treeNode.ForeColor];
                var strBG_Description = UtilColorcode.Descriptions[tuple.treeNode.BackColor];

                if (false == string.IsNullOrWhiteSpace(strFG_Description))
                {
                    ieDetail = ieDetail.Concat(new[]
                    { new LVitem_FolderDetailVM(new[] { "", strFG_Description }) { Foreground = tuple.treeNode.Foreground } });
                }

                if (false == string.IsNullOrWhiteSpace(strBG_Description))
                {
                    ieDetail = ieDetail.Concat(new[]
                    { new LVitem_FolderDetailVM(new[] { "", strBG_Description }) { Background = tuple.treeNode.Background } });
                }
#if DEBUG
                var nHashVersion = (Statics.DupeFileDictionary.AllListingsHashV2) ? "1 MB" : "4K";

                ieDetail = ieDetail.Concat(new[]
                { new LVitem_FolderDetailVM(new[] { nHashVersion + " All files hash", "" + tuple.treeNode.NodeDatum.Hash_AllFiles}) });
#endif
                Title = tuple.treeNode.Text;
                Add(ieDetail);
            });
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
