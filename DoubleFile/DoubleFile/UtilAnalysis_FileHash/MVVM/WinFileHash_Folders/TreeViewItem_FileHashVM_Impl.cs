using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class TreeViewItem_FileHashVM
    {
        void DoTreeSelect()
        {
            if (false == _bSelected)
            {
                return;
            }

            new Local.TreeSelect(_datum, _TVVM._dictVolumeInfo, false, false).DoThreadFactory();
        }
    }
}
