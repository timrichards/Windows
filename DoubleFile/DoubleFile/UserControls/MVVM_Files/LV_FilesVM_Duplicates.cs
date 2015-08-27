using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class LV_FilesVM : IDisposable
    {
        void ShowDuplicates_SelectedFileChangedOnNext(int nInitiator)
        {
            Util.Write("I");
            _cts.Cancel();

            Util.ThreadMake(() => 
            {
                try
                {
                    _selectedItem.DupIndex = 0;
                    TreeFileSelChanged();
                    _selectedItem.RaisePropertyChanged("Duplicate");
                    _selectedItem.RaisePropertyChanged("Parent");
                }
                catch (OperationCanceledException) { }

                SelectedFileChangedOnNext(new SelectedFileChanged(_selectedItem.LSdupDirFileLines, _selectedItem.FileLine, _treeNode), nInitiator);
            });
        }

        void TreeFileSelChanged()
        {
            if (null != _selectedItem.LSdupDirFileLines)
                return;

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

            var lsDupDirFileLines = new ConcurrentBag<Tuple<LVitemProject_Updater<bool>, IReadOnlyList<string>>> { };

            if (null == _selectedItem.LSduplicates)
                return;

            var lsKeys = _selectedItem.LSduplicates
                .GroupBy(duplicate => duplicate.LVitemProjectVM).ToList();

            Util.ParallelForEach(99656,
                lsKeys,
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
                var lvItemProject_Updater = new LVitemProject_Updater<bool>(g.Key, null);

                foreach (var strLine
                    in lvItemProject_Updater.ListingFile
                    .ReadLinesWait(99651))
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

                            var asLine =
                                strFileLine.Split('\t')
                                .Skip(2)
                                .ToArray();

                            asLine[0] = strLine.Split('\t')[2];
                            lsDupDirFileLines.Add(Tuple.Create(lvItemProject_Updater, (IReadOnlyList<string>)asLine));
                        }

                        lsFilesInDir.Clear();

                        if (0 == lsLineNumbers.Count)
                            break;
                    }
                }
            });

            _selectedItem.LSdupDirFileLines = lsDupDirFileLines.ToList();
        }

        CancellationTokenSource
            _cts = new CancellationTokenSource();
    }
}
