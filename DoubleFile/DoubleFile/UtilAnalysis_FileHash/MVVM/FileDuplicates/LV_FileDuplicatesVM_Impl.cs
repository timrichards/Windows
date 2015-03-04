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
            Icmd_Goto = new RelayCommand(param => Goto(), param => SelectedAny());
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates)
        {
            Items.Clear();

            if (null == lsDuplicates)
                return;

            var laoLines = new ConcurrentBag<object[]>();

            Parallel.ForEach(
                lsDuplicates
                .GroupBy(duplicate => duplicate.LVitemProjectVM), g =>
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
                    .ReadLines(g.Key.ListingFile))
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
                            laoLines.Add(new object[] { strFileLine, strLine.Split('\t')[2], g.Key });

                        lsFilesInDir.Clear();
 
                        if (0 == lsLineNumbers.Count)
                            break;
                   }
                }
            });

            foreach (var aoLine in laoLines)
            {
                var lvItem = new LVitem_FileDuplicatesVM(new[] { ((string)aoLine[0]).Split('\t')[3], (string)aoLine[1] });

                lvItem.FileLine = (string)aoLine[0];
                lvItem.LVitem_ProjectVM = (LVitem_ProjectVM)aoLine[2];
                Add(lvItem, bQuiet: true);
            }

            RaiseItems();
        }

        internal void ClearItems()
        {
            UtilProject.UIthread(Items.Clear);
        }

        internal void Goto()
        {
            Selected().FirstOnlyAssert(lvItem =>
            {
                //lvItem.LVitem_ProjectVM;
                //lvItem.Path;
                //lvItem.Filename;
                MBoxStatic.ShowDialog("Goto()");
            });
        }

        GlobalData_Base _gd = null;
    }
}
