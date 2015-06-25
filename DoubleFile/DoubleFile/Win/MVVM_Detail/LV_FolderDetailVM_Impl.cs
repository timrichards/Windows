using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FolderDetailVM : IDisposable
    {
        internal LV_FolderDetailVM()
        {
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Subscribe(TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));
        }

        void TreeSelect_FolderDetailUpdated(Tuple<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("G");
            Title = null;
            ClearItems();

            if (null == tuple.Item2)
                return;

            Util.UIthread(() =>
            {
                var ieDetail =
                    tuple.Item1
                    .Select(ieLine => new LVitem_FolderDetailVM(ieLine.ToList()));

                var strFG_Description = UtilColor.Description[tuple.Item2.ForeColor];
                var strBG_Description = UtilColor.Description[tuple.Item2.BackColor];

                if (false == string.IsNullOrEmpty(strFG_Description))
                {
                    ieDetail = ieDetail.Concat(new[]
                    { new LVitem_FolderDetailVM(new[] { "", strFG_Description }) { Foreground = tuple.Item2.Foreground } });
                }

                if (false == string.IsNullOrEmpty(strBG_Description))
                {
                    ieDetail = ieDetail.Concat(new[]
                    { new LVitem_FolderDetailVM(new[] { "", strBG_Description }) { Background = tuple.Item2.Background } });
                }
#if DEBUG
                var nHashVersion = (App.FileDictionary.AllListingsHashV2) ? 128 : 4;

                ieDetail = ieDetail.Concat(new[]
                { new LVitem_FolderDetailVM(new[] { nHashVersion + "K Hash Parity", "" + tuple.Item2.NodeDatum.HashParity }) });
#endif
                Title = tuple.Item2.Text;
                Add(ieDetail);
            });
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposable);
        }

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
