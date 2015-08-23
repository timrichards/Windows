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
        //static internal IObservable<Tuple<Tuple<IReadOnlyList<string>, LocalTreeNode>, int>>
        //    UpdateFileDetail => _updateFileDetail;
        //static readonly LocalSubject<Tuple<IReadOnlyList<string>, LocalTreeNode>> _updateFileDetail = new LocalSubject<Tuple<IReadOnlyList<string>, LocalTreeNode>>();
        //static void UpdateFileDetailOnNext(Tuple<IReadOnlyList<string>, LocalTreeNode> value, int nInitiator) => _updateFileDetail.LocalOnNext(value, 99847, nInitiator);

        void ShowDuplicates()
        {
            Util.Write("I");
            _cts.Cancel();

//            UpdateFileDetailOnNext(Tuple.Create(asFileLine, _treeNode), initiatorTuple.Item2);

            Util.ThreadMake(() => 
            {
                try
                {
                    TreeFileSelChanged();
                }
                catch (OperationCanceledException) { }
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

            var lsDupDirFileLines = new ConcurrentBag<IReadOnlyList<string>> { };

            Util.ParallelForEach(
                _selectedItem.LSduplicates
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

                foreach (var strLine
                    in g.Key.ListingFile
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

                            var asLine =
                                strFileLine.Split('\t')
                                .Skip(2)
                                .ToArray();

                            asLine[0] = strLine.Split('\t')[2];
                            lsDupDirFileLines.Add(asLine);
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
