using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    partial class LV_FileDetailVM : IDisposable
    {
        internal LV_FileDetailVM()
        {
            Icmd_Copy = new RelayCommand(Copy, () => false == string.IsNullOrEmpty(LocalPath));
            _lsDisposable.Add(WinDuplicatesVM.UpdateFileDetail.Subscribe(WinDuplicatesVM_UpdateFileDetail));
            _lsDisposable.Add(LV_FilesVM.SelectedFileChanged.Subscribe(LV_FilesVM_SelectedFileChanged));

            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Subscribe(initiatorTuple =>
            {
                var tuple = initiatorTuple.Item1;

                Util.Write("E"); if (null != tuple.Item2) LocalPath_Set(tuple.Item2);
            }));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                LocalPath_Set(folderDetail.Item2);
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposable);
        }

        void Copy()
        {
            Clipboard.SetText(LocalPath);
        }

        void LV_FilesVM_SelectedFileChanged(Tuple<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode>, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;
            var item2 = (null != tuple) ? tuple.Item2 : null;
            var item3 = (null != tuple) ? tuple.Item3 : null;

            WinDuplicatesVM_UpdateFileDetail(Tuple.Create(Tuple.Create(item2, item3), initiatorTuple.Item2));
        }

        void WinDuplicatesVM_UpdateFileDetail(Tuple<Tuple<IEnumerable<string>, LocalTreeNode>, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("F");
            LocalPath_Set();
            Title = null;
            ClearItems();

            if (null == tuple.Item1)
                return;

            var asFileLine =
                tuple.Item1
                .ToArray();

            Title = asFileLine[0];
            LocalPath_Set(tuple.Item2, asFileLine[0]);
            asFileLine[3] = Util.DecodeAttributes(asFileLine[3]);

            if ((asFileLine.Length > FileParse.knColLengthLV) &&
                (false == string.IsNullOrWhiteSpace(asFileLine[FileParse.knColLengthLV])))
            {
                asFileLine[FileParse.knColLengthLV] =
                    Util.FormatSize(asFileLine[FileParse.knColLengthLV], bBytes: true);
            }

            Util.UIthread(() =>
            {
                var kasHeader = new[] { "Filename", "Created", "Modified", "Attributes", "Length", "Error 1", "Error 2" };
                var nMax = Math.Min(asFileLine.Length, kasHeader.Length);

                for (var i = 1; i < nMax; ++i)
                {
                    if (string.IsNullOrWhiteSpace(asFileLine[i]))
                        continue;

                    Add(new LVitem_FileDetailVM(new[] { kasHeader[i], asFileLine[i] }), bQuiet: true);
                }
            });

            RaiseItems();
        }

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
