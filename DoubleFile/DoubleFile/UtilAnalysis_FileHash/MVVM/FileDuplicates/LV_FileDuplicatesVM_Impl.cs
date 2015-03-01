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

                lsLineNumbers.Sort();               // jic already sorted upstream at A

                int nLine = 0;
                List<string> lsFilesInDir = new List<string>();
                var nMatchLine = lsLineNumbers[0];

                foreach (var strLine
                    in File.ReadLines(g.Key))
                {
                    ++nLine;

                    if (nLine == nMatchLine)
                    {
                        lsFilesInDir.Add(strLine);
                        lsLineNumbers.RemoveAt(0);
                        nMatchLine = (lsLineNumbers.Count > 0) ? lsLineNumbers[0] : -1;
                    }
                    else if (strLine.StartsWith(FileParse.ksLineType_Directory))
                    {
                        foreach (var strFileLine in lsFilesInDir)
                            lsLines.Add(new[] { strFileLine.Split('\t')[3], strLine.Split('\t')[2] });

                        lsFilesInDir.Clear();
 
                        if (0 == lsLineNumbers.Count)
                            break;
                   }
                }

                MBoxStatic.Assert(99908, 0 == lsLineNumbers.Count);
            });

            UtilProject.UIthread(() => 
            {
                foreach (var strLine in lsLines)
                {
                    Add(new LVitem_FileDuplicatesVM(new[] { strLine[0], strLine[1] }), bQuiet: true);
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
