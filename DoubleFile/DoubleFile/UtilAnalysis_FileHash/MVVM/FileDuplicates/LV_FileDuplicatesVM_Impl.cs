using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class LV_FileDuplicatesVM
    {
        internal LV_FileDuplicatesVM(GlobalData_Base gd)
        {
            _gd = gd;
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates)
        {
            Items.Clear();

            if (null == lsDuplicates)
                return;

            var lasLines = new ConcurrentBag<string[]>();

            Parallel.ForEach(
                lsDuplicates
                .GroupBy(duplicate => duplicate.LVitemProjectVM.ListingFile), g =>
            {
                var lsLineNumbers = new List<int>();

                foreach (var duplicate in g)
                    lsLineNumbers.Add(duplicate.LineNumber);

                lsLineNumbers.Sort();               // jic already sorted upstream at A

                var nLine = 0;
                var lsFilesInDir = new List<string>();
                var nMatchLine = lsLineNumbers[0];

                foreach (var strLine
                    in File
                    .ReadLines(g.Key))
                {
                    ++nLine;

                    if (nLine == nMatchLine)
                    {
                        lsFilesInDir.Add(strLine);
                        lsLineNumbers.RemoveAt(0);
                        nMatchLine = (0 < lsLineNumbers.Count) ? lsLineNumbers[0] : -1;
                    }
                    else if ((0 < lsFilesInDir.Count) &&
                        strLine.StartsWith(FileParse.ksLineType_Directory))
                    {
                        foreach (var strFileLine in lsFilesInDir)
                            lasLines.Add(new[] { strFileLine, strLine.Split('\t')[2] });

                        lsFilesInDir.Clear();
 
                        if (0 == lsLineNumbers.Count)
                            break;
                   }
                }
            });

            foreach (var asLine in lasLines)
            {
                var lvItem = new LVitem_FileDuplicatesVM(new[] { asLine[0].Split('\t')[3], asLine[1] });

                lvItem.FileLine = asLine[0];
                Add(lvItem, bQuiet: true);
            }

            RaiseItems();
        }

        internal void ClearItems()
        {
            UtilProject.UIthread(Items.Clear);
        }

        GlobalData_Base _gd = null;
    }
}
