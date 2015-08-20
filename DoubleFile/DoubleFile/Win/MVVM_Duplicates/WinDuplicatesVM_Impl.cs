using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class WinDuplicatesVM : IDisposable
    {
        static internal IObservable<Tuple<Tuple<LVitem_ProjectVM, string, string>, int>>
            GoToFile => _goToFile;
        static readonly LocalSubject<Tuple<LVitem_ProjectVM, string, string>> _goToFile = new LocalSubject<Tuple<LVitem_ProjectVM, string, string>>();
        static void GoToFileOnNext(Tuple<LVitem_ProjectVM, string, string> value) => _goToFile.LocalOnNext(value, 99848);

        static internal IObservable<Tuple<Tuple<IReadOnlyList<string>, LocalTreeNode>, int>>
            UpdateFileDetail => _updateFileDetail;
        static readonly LocalSubject<Tuple<IReadOnlyList<string>, LocalTreeNode>> _updateFileDetail = new LocalSubject<Tuple<IReadOnlyList<string>, LocalTreeNode>>();
        static void UpdateFileDetailOnNext(Tuple<IReadOnlyList<string>, LocalTreeNode> value, int nInitiator) => _updateFileDetail.LocalOnNext(value, 99847, nInitiator);

        internal WinDuplicatesVM
            Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
            _nicknameUpdater.UpdateViewport(UseNicknames);
            _lsDisposable.Add(LV_FilesVM.SelectedFileChanged.LocalSubscribe(99704, LV_FilesVM_SelectedFileChanged));

            var lastSelectedFile = LV_FilesVM.LastSelectedFile;

            if (null != lastSelectedFile)
                LV_FilesVM_SelectedFileChanged(Tuple.Create(lastSelectedFile, 0));

            return this;
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        void LV_FilesVM_SelectedFileChanged(Tuple<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IReadOnlyList<string>, LocalTreeNode>, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("I");
            _cts.Cancel();

            LocalTreeNode treeNode = null;
            IEnumerable<FileDictionary.DuplicateStruct> ieDuplicates = null;
            IReadOnlyList<string> asFileLine = null;

            if (null != tuple)
            {
                treeNode = tuple.Item3;
                ieDuplicates = tuple.Item1;
                asFileLine = tuple.Item2;
            }

            _treeNode = treeNode;
            UpdateFileDetailOnNext(Tuple.Create(asFileLine, _treeNode), initiatorTuple.Item2);
            SelectedItem_Set(null);
            ClearItems();

            if (null == ieDuplicates)
                return;

            Util.ThreadMake(() => 
            {
                try
                {
                    TreeFileSelChanged(ieDuplicates, asFileLine);
                }
                catch (OperationCanceledException) { }
            });
        }

        void TreeFileSelChanged(IEnumerable<FileDictionary.DuplicateStruct> lsDuplicates, IEnumerable<string> ieFileLine)
        {
            {
                var nCheck = 0;

                while ((false == _cts.IsCancellationRequested) &&
                    (10 > nCheck))
                {
                    Util.WriteLine(nCheck + " false == _cts.IsCancellationRequested");
                    Util.Block(100);
                    ++nCheck;
                }

                if (10 <= nCheck)
                {
                    Util.Assert(99905, false);
                    return;     // from method
                }
            }

            var lsLVitems = new ConcurrentBag<LVitem_FileDuplicatesVM>();

            Util.ParallelForEach(
                lsDuplicates
                    .GroupBy(duplicate => duplicate.LVitemProjectVM),
                new ParallelOptions { CancellationToken = (_cts = new CancellationTokenSource()).Token },
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
                var lvItemProject_Updater = new LVitemProject_Updater<bool>(g.Key, _nicknameUpdater);

                foreach (var strLine
                    in lvItemProject_Updater.ListingFile
                    .ReadLines())
                {
                    if (_cts.IsCancellationRequested)
                        return;     // from lambda

                    ++nLine;

                    if (nLine == nMatchLine)
                    {
                        lsFilesInDir.Add(strLine);
                        lsLineNumbers.RemoveAt(0);
                        nMatchLine = 0 < lsLineNumbers.Count ? lsLineNumbers[0] : -1;
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

                                LVitemProject_Updater = lvItemProject_Updater
                            });
                        }

                        lsFilesInDir.Clear();

                        if (0 == lsLineNumbers.Count)
                            break;
                    }
                }
            });

            if (false == _cts.IsCancellationRequested)
                Util.UIthread(99812, () => Add(lsLVitems));
        }

        internal void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99901, false);    // binding should dim the button
                return;
            }

            GoToFileOnNext(Tuple.Create((LVitem_ProjectVM)_selectedItem.LVitemProject_Updater, _selectedItem.SubItems[0], _selectedItem.Filename));
        }

        ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>();
        CancellationTokenSource
            _cts = new CancellationTokenSource();
        LocalTreeNode
            _treeNode = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
