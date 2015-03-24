using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class WinDoubleFile_DuplicatesVM : IDisposable
    {
        static internal event Action<LVitem_ProjectVM, string, string> GoToFile;
        static internal event Action<string> UpdateFileDetail;

        internal WinDoubleFile_DuplicatesVM()
        {
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
            LV_DoubleFile_FilesVM.SelectedFileChanged += TreeFileSelChanged;
        }

        public void Dispose()
        {
            LV_DoubleFile_FilesVM.SelectedFileChanged -= TreeFileSelChanged;
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, string strFileLine)
        {
            if (null != UpdateFileDetail)
                UpdateFileDetail(strFileLine);

            _selectedItem = null;
            UtilProject.UIthread(Items.Clear);

            if (null == lsDuplicates)
                return;

            var lsLVitems = new ConcurrentBag<LVitem_FileDuplicatesVM>();

            Parallel.ForEach(
                lsDuplicates
                .GroupBy(duplicate => duplicate.LVitemProjectVM), g =>
            {
                var lsLineNumbers =
                    g
                    .Select(duplicate => duplicate.LineNumber)
                    .OrderBy(x => x)        // jic already sorted upstream at A
                    .ToList();

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
                        foreach (var strFileLineA in lsFilesInDir)
                        {
                            lsLVitems.Add(new LVitem_FileDuplicatesVM(new[] { strFileLineA.Split('\t')[3], strLine.Split('\t')[2] })
                            {
                                FileLine = strFileLineA,
                                LVitem_ProjectVM = g.Key
                            });
                        }

                        lsFilesInDir.Clear();
 
                        if (0 == lsLineNumbers.Count)
                            break;
                   }
                }
            });

            UtilProject.UIthread(() => Add(lsLVitems));
        }

        internal void Goto()
        {
            if (null == GoToFile)
                return;

            if (null == _selectedItem)
            {
                MBoxStatic.Assert(99901, false);    // binding should dim the button
                return;
            }

            GoToFile(_selectedItem.LVitem_ProjectVM, _selectedItem.Path, _selectedItem.Filename);
        }
    }
}
