using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class TreeViewItem_DoubleFileVM
    {
        void DoTreeSelect()
        {
            if (false == _isSelected)
                return;

            new TreeSelect(_datum).DoThreadFactory();
        }
    }
}
