using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class TreeViewItem_DoubleFileVM
    {
        static internal event Action<string> SelectedFile = null;

        void DoTreeSelect()
        {
            if (false == _isSelected)
                return;

            new Local.TreeSelect(_datum).DoThreadFactory();
        }

        internal void GoToFile(IEnumerable<string> asPath, string strFile)
        {
            if ((0 == asPath.Count()) ||
                (1 == asPath.Count()) && string.IsNullOrWhiteSpace(asPath.ElementAt(0)))
            {
                SelectedItem_Set();

                if (null != SelectedFile)
                    SelectedFile(strFile);

                return;
            }

            asPath.First(strPath =>
                Items
                    .Where(item => strPath == item._datum.Text)     // that 200 char padding for the selection hack
                    .FirstOnlyAssert(item =>
                        item.GoToFile(asPath.Skip(1), strFile)));
        }
    }
}
