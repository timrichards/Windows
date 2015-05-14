using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class WinDoubleFile_DuplicatesVM : IDisposable
    {
        static internal IObservable<Tuple<LVitem_ProjectVM, string, string>>
            GoToFile { get { return _goToFile.AsObservable(); } }
        static readonly Subject<Tuple<LVitem_ProjectVM, string, string>> _goToFile = new Subject<Tuple<LVitem_ProjectVM, string, string>>();
        static void GoToFileOnNext(Tuple<LVitem_ProjectVM, string, string> value) { _goToFile.LocalOnNext(value, 99848); }

        static internal IObservable<Tuple<IEnumerable<string>, LocalTreeNode>>
            UpdateFileDetail { get { return _updateFileDetail.AsObservable(); } }
        static readonly Subject<Tuple<IEnumerable<string>, LocalTreeNode>> _updateFileDetail = new Subject<Tuple<IEnumerable<string>, LocalTreeNode>>();
        static void UpdateFileDetailOnNext(Tuple<IEnumerable<string>, LocalTreeNode> value) { _updateFileDetail.LocalOnNext(value, 99847); }

        internal WinDoubleFile_DuplicatesVM()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            _lsDisposable.Add(LV_DoubleFile_FilesVM.SelectedFileChanged.Subscribe(LV_DoubleFile_FilesVM_SelectedFileChanged));
        }

        public void Dispose()
        {
            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        void LV_DoubleFile_FilesVM_SelectedFileChanged(Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode> tuple)
        {
            UtilDirList.Write("I");
            if (null != _cts)
                _cts.Cancel();

            LocalTreeNode treeNode = null;
            IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates = null;
            IEnumerable<string> ieFileLine = null;

            if (null != tuple)
            {
                treeNode = tuple.Item3;
                lsDuplicates = tuple.Item1;
                ieFileLine = tuple.Item2;
            }

            _treeNode = treeNode;
            UpdateFileDetailOnNext(Tuple.Create(ieFileLine, _treeNode));
            SelectedItem_Set(null);
            UtilProject.UIthread(ClearItems);

            if (null == lsDuplicates)
                return;

            new Thread(() => 
            {
                try
                {
                    TreeFileSelChanged(lsDuplicates, ieFileLine);
                }
                catch (OperationCanceledException) { }
            }).Start();
        }

        void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, IEnumerable<string> ieFileLine)
        {
            {
                var nCheck = 0;

                while ((null != _cts) &&
                    (false == _cts.IsCancellationRequested) &&
                    (10 > nCheck))
                {
                    UtilProject.WriteLine(nCheck + " false == _cts.IsCancellationRequested");
                    Thread.Sleep(100);
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

            if (false == _cts.IsCancellationRequested)
                UtilProject.UIthread(() => Add(lsLVitems));

            _cts = null;
        }

        internal void GoTo()
        {
            if (null == _selectedItem)
            {
                MBoxStatic.Assert(99901, false);    // binding should dim the button
                return;
            }

            GoToFileOnNext(Tuple.Create(_selectedItem.LVitem_ProjectVM, _selectedItem.Path, _selectedItem.Filename));
        }

        CancellationTokenSource
            _cts = null;
        LocalTreeNode
            _treeNode = null;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
