using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DoubleFile
{
    partial class LV_FileDuplicatesVM : IDisposable
    {
        internal LV_FileDuplicatesVM(GlobalData_Base gd)
        {
            _gd = gd;
            TreeViewItem_FileHashVM.SelectedItemChanged += DuplicateFileSelChanged;
        }

        public void Dispose()
        {
            TreeViewItem_FileHashVM.SelectedItemChanged -= DuplicateFileSelChanged;
        }

        internal void TreeFileSelChanged(IReadOnlyList<FileDictionary.DuplicateStruct> lsDuplicates)
        {
            UtilProject.WriteLine("TreeFileSelChanged");
            UtilProject.UIthread(Items.Clear);

            if (null == lsDuplicates)
                return;

            var lsLines = new ConcurrentBag<string[]>();

            lsDuplicates
                .GroupBy(duplicate => duplicate.LVitemProjectVM.ListingFile)
                .AsParallel()
                .ForEach(g =>
            {
                List<int> lsLineNumbers = new List<int>();

                foreach (var duplicate in g)
                {
                    lsLineNumbers.Add(duplicate.LineNumber);
                }

                MBoxStatic.Assert(99902, 0 < lsLineNumbers.Count);
                lsLineNumbers.Sort();               // jic already sorted upstream at A

                int nLine = 0;
                string strDirLine = null;
                var bRootTest = true;

                foreach (var strLine
                    in File.ReadLines(g.Key))
                {
                    ++nLine;

                    if (0 == lsLineNumbers.Count)
                        break;

                    var nMatchLine = lsLineNumbers[0];
                    var bRoot = (bRootTest && 
                        strLine.StartsWith(FileParse.ksLineType_VolumeInfo_Root));

                    bRootTest &= (false == bRoot);

                    if (bRoot || strLine.StartsWith(FileParse.ksLineType_Directory))
                    {
                        if (nLine == nMatchLine)
                        {
                            MBoxStatic.Assert(99903, false);
                            lsLineNumbers.RemoveAt(0);
                        }

                        strDirLine = strLine;       // clobber
                    }
                    else if (nLine == nMatchLine)
                    {
#if (DEBUG)
                        MBoxStatic.Assert(99905, "" + nLine == strLine.Split('\t')[1]);
#endif
                        lsLines.Add(new[] { strLine, strDirLine });
                        lsLineNumbers.RemoveAt(0);
                    }
                }

                MBoxStatic.Assert(99904, 0 == lsLineNumbers.Count);
            });

            UtilProject.UIthread(() => 
            {
                foreach (var strLine in lsLines)
                {
                    Add(new LVitem_FileDuplicatesVM(new[] { strLine[0].Split('\t')[3], strLine[1].Split('\t')[2] }), bQuiet: true);
                }

                RaiseItems();
            });
        }

        void DuplicateFileSelChanged(IEnumerable<string> lsFiles, string strListingFile)
        {
             UtilProject.UIthread(Items.Clear);
        }

        GlobalData_Base _gd = null;
    }
}
