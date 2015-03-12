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

        internal WinDoubleFile_DuplicatesVM(GlobalData_Base gd)
        {
            _gd = gd;
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != SelectedItem);
            Local.TreeSelect.FileListUpdated += TreeSelect_FileList;
        }

        public void Dispose()
        {
            if ((null != _winDoubleFile_Detail) &&
                (false == _winDoubleFile_Detail.LocalIsClosed))
            {
                _winDoubleFile_Detail.Close();
            }

            Local.TreeSelect.FileListUpdated -= TreeSelect_FileList;
        }

        internal bool ShowWindows()
        {
            if ((null != _winDoubleFile_Detail) &&
                (false == _winDoubleFile_Detail.LocalIsClosed))
            {
                _winDoubleFile_Detail.ShowWindows();
                return false;
            }

            (_winDoubleFile_Detail = new WinDoubleFile_Detail(_gd)).Show();
            return true;
        }

        void TreeSelect_FileList(IEnumerable<string> lsFileLines, string strListingFile)
        {
            UtilProject.UIthread(Items.Clear);
            _winDoubleFile_Detail.UpdateFileDetail(/*clear items*/);
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, string strFileLine)
        {
            _winDoubleFile_Detail.UpdateFileDetail(strFileLine);

            Items.Clear();

            if (null == lsDuplicates)
                return;

            var laoLines = new ConcurrentBag<Tuple<string, string, LVitem_ProjectVM>>();

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
                        foreach (var strFileLineA in lsFilesInDir)
                            laoLines.Add(Tuple.Create(strFileLineA, strLine.Split('\t')[2], g.Key));

                        lsFilesInDir.Clear();
 
                        if (0 == lsLineNumbers.Count)
                            break;
                   }
                }
            });

            foreach (var aoLine in laoLines)
            {
                var lvItem = new LVitem_FileDuplicatesVM(new[] { aoLine.Item1.Split('\t')[3], aoLine.Item2 });

                lvItem.FileLine = aoLine.Item1;
                lvItem.LVitem_ProjectVM = aoLine.Item3;
                Add(lvItem, bQuiet: true);
            }

            RaiseItems();
        }

        internal void Goto()
        {
            if (null == GoToFile)
                return;

            if (null == SelectedItem)
            {
                MBoxStatic.Assert(99901, false);    // binding should dim the button
            }

            GoToFile(SelectedItem.LVitem_ProjectVM, SelectedItem.Path, SelectedItem.Filename);
        }

        WinDoubleFile_Detail
            _winDoubleFile_Detail = null;
        GlobalData_Base
            _gd = null;
    }
}
