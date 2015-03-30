using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class WinDoubleFile_DuplicatesVM : IDisposable
    {
        static internal event Action<LVitem_ProjectVM, string, string> GoToFile;
        static internal event Action<IEnumerable<string>> UpdateFileDetail;

        internal WinDoubleFile_DuplicatesVM()
        {
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
            LV_DoubleFile_FilesVM.SelectedFileChanged += TreeFileSelChanged;
        }

        public void Dispose()
        {
            LV_DoubleFile_FilesVM.SelectedFileChanged -= TreeFileSelChanged;

            if (null != _timer)
                _timer.Dispose();
        }

        internal void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, IEnumerable<string> ieFileLine)
        {
            if (null != _timer)
                _timer.Dispose();

            if (null != _cts)
                _cts.Cancel();

            _timer = new LocalTimer(33.0, () =>
            {
                _timer.Dispose();
                TreeFileSelChangedA(lsDuplicates, ieFileLine);
            }).Start();
        }

        void TreeFileSelChangedA(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, IEnumerable<string> ieFileLine)
        {
            if (null != UpdateFileDetail)
                UpdateFileDetail(ieFileLine);

            SelectedItem_Set(null);
            UtilProject.UIthread(Items.Clear);

            if (null == lsDuplicates)
                return;

            {
                var nCheck = 0;

                while ((null != _cts) &&
                    (false == _cts.IsCancellationRequested) &&
                    (10 > nCheck))
                {
                    UtilProject.WriteLine(nCheck + "false == _cts.IsCancellationRequested");
                    Thread.Sleep(20);
                    ++nCheck;
                }

                if (10 <= nCheck)
                {
                    MBoxStatic.Assert(99905, false);
                    return;     // from method
                }
            }

            var lsLVitems = new ConcurrentBag<LVitem_FileDuplicatesVM>();

            Parallel.ForEach(
                lsDuplicates
                    .GroupBy(duplicate => duplicate.LVitemProjectVM),
                new ParallelOptions() { CancellationToken = (_cts = new CancellationTokenSource()).Token },
                g =>
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
                        if (_cts.IsCancellationRequested)
                            return;     // from lambda

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
                            {
                                if (_cts.IsCancellationRequested)
                                    return;     // from lambda

                                lsLVitems.Add(new LVitem_FileDuplicatesVM(new[] { strLine.Split('\t')[2] })
                                {
                                    FileLine = strFileLine.Split('\t')
                                        .Skip(3)                    // makes this an LV line: knColLengthLV
                                        .ToArray(),

                                    LVitem_ProjectVM = g.Key
                                });
                            }

                            lsFilesInDir.Clear();

                            if (0 == lsLineNumbers.Count)
                                break;
                        }
                    }
                });

            if (_cts.IsCancellationRequested)
                return;

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

        LocalTimer _timer = null;
        CancellationTokenSource _cts = null;
    }
}
