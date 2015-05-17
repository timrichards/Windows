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
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Subscribe(tuple => { UtilDirList.Write("E"); if (null != tuple.Item2) LocalPath_Set(tuple.Item2); }));
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

        void LV_DoubleFile_FilesVM_SelectedFileChanged(Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode> tuple = null)
        {
            WinDoubleFile_DuplicatesVM_UpdateFileDetail(Tuple.Create(tuple.Item2, tuple.Item3));
        }

        void WinDoubleFile_DuplicatesVM_UpdateFileDetail(Tuple<IEnumerable<string>, LocalTreeNode> tuple = null)
        {
            UtilDirList.Write("F");
            _treeNode = tuple.Item2;
            LocalPath_Set();
            Title = null;
            UtilProject.UIthread(ClearItems);

            if (null == tuple.Item1)
                return;

            var asFileLine =
                tuple.Item1
                .ToArray();

            if (asFileLine[0] == Title)
                return;

            LocalPath_Set(_treeNode, asFileLine[0]);
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

                Title = asFileLine[0];
                RaiseItems();
            });
        }

        LocalTreeNode
            _treeNode = null;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
