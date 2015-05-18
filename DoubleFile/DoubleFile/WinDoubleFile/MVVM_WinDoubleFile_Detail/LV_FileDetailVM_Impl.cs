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
            _lsDisposable.Add(WinDoubleFile_DuplicatesVM.UpdateFileDetail.Subscribe(WinDoubleFile_DuplicatesVM_UpdateFileDetail));
            _lsDisposable.Add(LV_DoubleFile_FilesVM.SelectedFileChanged.Subscribe(LV_DoubleFile_FilesVM_SelectedFileChanged));

            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Subscribe(tupleA =>
            {
                var tuple = tupleA.Item1;

                UtilDirList.Write("E"); if (null != tuple.Item2) LocalPath_Set(tuple.Item2);
            }));
        }

        public void Dispose()
        {
            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        void Copy()
        {
            Clipboard.SetText(LocalPath);
        }

        void LV_DoubleFile_FilesVM_SelectedFileChanged(Tuple<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode>, int> tupleA)
        {
            var tuple = tupleA.Item1;
            var item2 = (null != tuple) ? tuple.Item2 : null;
            var item3 = (null != tuple) ? tuple.Item3 : null;

            WinDoubleFile_DuplicatesVM_UpdateFileDetail(Tuple.Create(Tuple.Create(item2, item3), tupleA.Item2));
        }

        void WinDoubleFile_DuplicatesVM_UpdateFileDetail(Tuple<Tuple<IEnumerable<string>, LocalTreeNode>, int> tupleA)
        {
            var tuple = tupleA.Item1;

            UtilDirList.Write("F");
            LocalPath_Set();
            Title = null;
            UtilProject.UIthread(ClearItems);

            if (null == tuple.Item1)
                return;

            var asFileLine =
                tuple.Item1
                .ToArray();

            Title = asFileLine[0];
            LocalPath_Set(tuple.Item2, asFileLine[0]);
            asFileLine[3] = UtilDirList.DecodeAttributes(asFileLine[3]);

            if ((asFileLine.Length > FileParse.knColLengthLV) &&
                (false == string.IsNullOrWhiteSpace(asFileLine[FileParse.knColLengthLV])))
            {
                asFileLine[FileParse.knColLengthLV] =
                    UtilDirList.FormatSize(asFileLine[FileParse.knColLengthLV], bBytes: true);
            }

            UtilProject.UIthread(() =>
            {
                var kasHeader = new[] { "Filename", "Created", "Modified", "Attributes", "Length", "Error 1", "Error 2" };
                var nMax = Math.Min(asFileLine.Length, kasHeader.Length);

                for (var i = 1; i < nMax; ++i)
                {
                    if (string.IsNullOrWhiteSpace(asFileLine[i]))
                        continue;

                    Add(new LVitem_FileDetailVM(new[] { kasHeader[i], asFileLine[i] }), bQuiet: true);
                }

                RaiseItems();
            });
        }

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
