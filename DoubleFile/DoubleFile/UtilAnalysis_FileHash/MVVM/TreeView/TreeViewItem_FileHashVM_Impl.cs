
using System;
using System.Collections.Generic;
namespace DoubleFile
{
    partial class TreeViewItem_FileHashVM
    {
        static internal event Action<IEnumerable<string>, string> TreeSelect_FileList;
        static internal event Action<IEnumerable<string[]>> TreeSelect_FolderDetail;
        static internal event Action<IEnumerable<string[]>> TreeSelect_VolumeDetail;

        void DoTreeSelect()
        {
            if ((false == _bSelected) ||
                (null == TreeSelect_FileList))
            {
                return;
            }

            new Local.TreeSelect(_datum, _TVVM._dictVolumeInfo, false, false,
                (lsFiles, strListingFile) => { if (null != TreeSelect_FileList) TreeSelect_FileList(lsFiles, strListingFile); },
                (lsDetail) => { if (null != TreeSelect_FolderDetail) TreeSelect_FolderDetail(lsDetail); },
                (lsDetail) => { if (null != TreeSelect_VolumeDetail) TreeSelect_VolumeDetail(lsDetail); }
                ).DoThreadFactory();
        }
    }
}
