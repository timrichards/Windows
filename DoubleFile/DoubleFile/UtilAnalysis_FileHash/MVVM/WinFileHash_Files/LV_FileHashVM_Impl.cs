using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FileHashVM : IDisposable
    {
        internal LV_FileHashVM(GlobalData_Base gd)
        {
            _gd = gd;
            Local.TreeSelect.FileListUpdated += TreeSelect_FileList;
        }

        public void Dispose()
        {
            Local.TreeSelect.FileListUpdated -= TreeSelect_FileList;
        }

        void TreeSelect_FileList(IEnumerable<string> lsFileLines, string strListingFile)
        {
            UtilProject.UIthread(() =>
            {
                Items.Clear();

                if (null == lsFileLines)
                    return;

                foreach (var strFileLine in lsFileLines)
                {
                    string strFilename = null;
                    var nLine = -1;
                    var lsDuplicates = _gd.FileDictionary.GetDuplicates(strFileLine, out strFilename, out nLine);
                    var nCount = (null != lsDuplicates) ? lsDuplicates.Count() - 1 : 0;
                    var strCount = (nCount > 0) ? "" + nCount : null;
                    var lvItem = new LVitem_FileHashVM(new[] { strFilename, strCount });

                    lvItem.FileLine = strFileLine;
                    lvItem.Solitary = (0 == nCount);

                    if (false == lvItem.Solitary)
                    {
                        lvItem.LSduplicates =
                            lsDuplicates
                            .Where(dupe =>
                                (dupe.LVitemProjectVM.ListingFile != strListingFile) ||    // exactly once every query
                                (dupe.LineNumber != nLine)
                            );

                        lvItem.SameVolume =
                            (1 ==
                            lsDuplicates
                                .GroupBy(duplicate => duplicate.LVitemProjectVM.Volume)
                                .Count());
                    }

                    Add(lvItem, bQuiet: true);
                }

                RaiseItems();
            });
        }

        GlobalData_Base _gd = null;
    }
}
