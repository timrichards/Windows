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
                    lsLineNumbers.Add(duplicate.LineNumber);

                MBoxStatic.Assert(99903, 0 < lsLineNumbers.Count);
                lsLineNumbers.Sort();               // jic already sorted upstream at A

                var ieMain =
                    File
                    .ReadLines(g.Key)
                    .Skip(lsLineNumbers[0] - 1);
                List<string> lsFilesInDir = new List<string>();

                while (true)
                {
                    lsFilesInDir.Add(
                        ieMain
                        .Take(1)
                        .ToArray()[0]);

                    var nLineNumber = lsLineNumbers[0];
                    var nNextFileLineNumber = -1;

                    lsLineNumbers.RemoveAt(0);

                    if (0 < lsLineNumbers.Count)
                        nNextFileLineNumber = lsLineNumbers[0];

                    ieMain =
                        ieMain
                        .SkipWhile(strLine =>
                            (false == strLine.StartsWith(FileParse.ksLineType_Directory) &&
                            (nLineNumber++ < nNextFileLineNumber - 1)));

                    var bBreak = false;

                    ieMain
                        .First(strLine =>
                    {
                        if (strLine.StartsWith(FileParse.ksLineType_Directory))
                        {
#if (DEBUG)
                            MBoxStatic.Assert(99905, strLine.Split('\t')[1] == "" + nLineNumber);
#endif
                            foreach (var strLineOut in lsFilesInDir)
                                lsLines.Add(new[] { strLineOut.Split('\t')[3], strLine.Split('\t')[2] });

                            lsFilesInDir.Clear();

                            if (0 < lsLineNumbers.Count)
                            {
                                ieMain =
                                    ieMain
                                    .Skip(nNextFileLineNumber - nLineNumber);

                                nLineNumber = nNextFileLineNumber;
                            }
                        }

                        if (0 == lsLineNumbers.Count)
                            bBreak = true;
                    });

                    if (bBreak)
                        break;
                }

                MBoxStatic.Assert(99904, 0 == lsLineNumbers.Count);
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
