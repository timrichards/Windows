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
            Icmd_Copy = new RelayCommand(param => Copy(), param => false == string.IsNullOrEmpty(LocalPath));
            WinDoubleFile_DuplicatesVM.UpdateFileDetail += WinDoubleFile_DuplicatesVM_UpdateFileDetail;
            TreeSelect.FolderDetailUpdated += TreeSelect_FolderDetailUpdated;
        }

        public void Dispose()
        {
            WinDoubleFile_DuplicatesVM.UpdateFileDetail -= WinDoubleFile_DuplicatesVM_UpdateFileDetail;
            TreeSelect.FolderDetailUpdated -= TreeSelect_FolderDetailUpdated;
        }

        void Copy()
        {
            Clipboard.SetText(LocalPath);
        }

        void TreeSelect_FolderDetailUpdated(IEnumerable<IEnumerable<string>> ieDetail, LocalTreeNode treeNode)
        {
            if (_treeNode != treeNode)
                LocalPath_Set(treeNode);
        }

        void WinDoubleFile_DuplicatesVM_UpdateFileDetail(IEnumerable<string> ieFileLine = null, LocalTreeNode treeNode = null)
        {
            _treeNode = treeNode;
            LocalPath_Set();
            Title = null;

            UtilProject.UIthread(Items.Clear);

            if (null == ieFileLine)
                return;

            var kasHeader = new[] { "Filename", "Created", "Modified", "Attributes", "Length", "Error 1", "Error 2" };

            var asFileLine =
                ieFileLine
                .ToArray();

            LocalPath_Set(treeNode, asFileLine[0]);
            asFileLine[3] = UtilDirList.DecodeAttributes(asFileLine[3]);

            if ((asFileLine.Length > FileParse.knColLengthLV) &&
                (false == string.IsNullOrWhiteSpace(asFileLine[FileParse.knColLengthLV])))
            {
                asFileLine[FileParse.knColLengthLV] =
                    UtilDirList.FormatSize(asFileLine[FileParse.knColLengthLV], bBytes: true);
            }

            UtilProject.UIthread(() =>
            {
                for (var i = 1; i < Math.Min(asFileLine.Length, kasHeader.Length); ++i)
                {
                    if (string.IsNullOrWhiteSpace(asFileLine[i]))
                        continue;

                    Add(new LVitem_FileDetailVM(new[] { kasHeader[i], asFileLine[i] }), bQuiet: true);
                }

                Title = asFileLine[0];
                RaiseItems();
            });
        }

        LocalTreeNode _treeNode = null;
    }
}
