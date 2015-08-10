using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Windows;
using System.IO.IsolatedStorage;

namespace DoubleFile
{
    interface ITreeStatus
    {
        void Status(LVitem_ProjectVM volStrings, LocalTreeNode rootNode = null, bool bError = false);
        void Done();
    }

    partial class Tree
    {
        partial class TreeRootNodeBuilder : TreeBase
        {
            internal
                TreeRootNodeBuilder(LVitem_ProjectVM lvItemProjectVM, TreeBase base_in)
                : base(base_in)
            {
                _lvItemProjectVM = lvItemProjectVM;
                Util.Assert(1301.2301m, _callbackWR != null);
            }

            DetailsDatum
                TreeSubnodeDetails(LocalTreeNode treeNode)
            {
                var datum = new DetailsDatum();

                if (null != treeNode.Nodes)
                {
                    foreach (var node in treeNode.Nodes)
                    {
                        if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                            _bThreadAbort)
                        {
                            return datum;
                        }

                        datum += TreeSubnodeDetails(node);
                    }
                }

                var nodeDatum = treeNode.NodeDatum;

                if (0 == (nodeDatum?.LineNo ?? 0))
                {
                    Util.Assert(99778, false);
                    return datum;
                }

                nodeDatum.TotalLength = (datum.TotalLength += nodeDatum.Length);
                nodeDatum.FileCountHere = nodeDatum.LineNo - nodeDatum.PrevLineNo - 1;
                nodeDatum.FileCountTotal = (datum.FileCountTotal += nodeDatum.FileCountHere);
                nodeDatum.SubDirs = (datum.SubDirs += (uint)(treeNode.Nodes?.Count ?? 0));

                datum.FolderScore =
                    datum.FolderScore.Zip(nodeDatum.FolderScore, (n1, n2) => n1 + n2)
                    .ToArray();

                nodeDatum.FolderScore =
                    datum.FolderScore.Zip(nodeDatum.FolderScore, (n1, n2) => n1 + n2)
                    .ToArray();

                if (0 < nodeDatum.FileCountHere)
                    ++datum.DirsWithFiles;

                nodeDatum.DirsWithFiles = datum.DirsWithFiles;

                var lsTreeNodes = _dictNodes.TryGetValue(nodeDatum.Key);

                if (null != lsTreeNodes)
                {
                    lock (lsTreeNodes)
                        lsTreeNodes.Add(treeNode);
                }
                else if (0 < nodeDatum.TotalLength)
                    _dictNodes[nodeDatum.Key] = new List<LocalTreeNode> { treeNode };

                return datum;
            }

            internal TreeRootNodeBuilder
                DoThreadFactory()
            {
                _thread = Util.ThreadMake(Go);
                return this;
            }

            internal void
                Join() => _thread.Join();

            internal void
                Abort()
            {
                _bThreadAbort = true;
                _thread.Abort();
            }

            void
                Go()
            {
                var dtStart = DateTime.Now;

                if (_lvItemProjectVM.CanLoad == false)
                {
                    Util.Assert(1301.2307m, false);    // guaranteed by caller
                    return;
                }

                {
                    var bValid = false;
                    var bAttemptConvert = false;

                    for (;;)
                    {
                        bValid =
                            ValidateFile(_lvItemProjectVM.ListingFile)
                            .Item1;

                        if (bValid || bAttemptConvert)
                            break;

                        if (false == LocalIsoStore.FileExists(StrFile_01(_lvItemProjectVM.ListingFile)))
                            break;

                        try
                        {
                            LocalIsoStore.DeleteFile(StrFile_01(_lvItemProjectVM.ListingFile));
                        }
                        catch (IsolatedStorageException) { }

                        bAttemptConvert = true;
                    }

                    if (false == bValid)
                    {
                        MBoxStatic.ShowDialog("Bad file: " + _lvItemProjectVM.ListingFile, "Tree");
                        StatusCallback(_lvItemProjectVM, bError: true);
                        return;
                    }
                }

                ulong nVolFree = 0;
                ulong nVolLength = 0;

                {
                    var ieDriveInfo =
                        _lvItemProjectVM.ListingFile
                        .ReadLines()
                        .Where(s => s.StartsWith(ksLineType_VolumeInfo));

                    var strBuilder = new StringBuilder();
                    var nIx = -1;

                    foreach (var asLine
                        in ieDriveInfo
                        .Select(strLine => strLine.Split('\t')))
                    {
                        ++nIx;

                        if (3 < asLine.Length)
                            strBuilder.Append(asLine[2]);
                        else if (2 < asLine.Length)
                            strBuilder.Append(kasDIlabels[nIx]);
                        else
                            continue;

                        var s = asLine[asLine.Length - 1];

                        strBuilder.AppendLine('\t' + s);

                        if ((5 == nIx) &&
                            (false == string.IsNullOrWhiteSpace(s)))
                        {
                            nVolFree = ("" + s).ToUlong();
                        }
                        else if ((6 == nIx) &&
                            (false == string.IsNullOrWhiteSpace(s)))
                        {
                            nVolLength = ("" + s).ToUlong();
                        }
                    }

                    lock (_dictDriveInfo)
                    {
                        if (null != _dictDriveInfo.TryGetValue(_lvItemProjectVM.ListingFile))
                        {
                            Util.Assert(1301.2308m, false);
                            _dictDriveInfo.Remove(_lvItemProjectVM.ListingFile);
                        }

                        _dictDriveInfo.Add(_lvItemProjectVM.ListingFile, ("" + strBuilder).Trim('\r', '\n'));
                    }
                }

                var dirData =
                    _lvItemProjectVM.ListingFile
                    .ReadLines()
                    .Where(s => s.StartsWith(ksLineType_Start))
                    .Select(s => new DirData((s.Split('\t')[1]).ToInt()))
                    .FirstOnlyAssert();

                var dt = DateTime.Now;
                var folderScore = new[] { 0U, 0U, 0U };  // Weighted folder scores: HashParity (random); largest; smallest

                var nHashColumn =
                    Statics.FileDictionary.AllListingsHashV2
                    ? 11
                    : 10;

                foreach (var strLine in _lvItemProjectVM.ListingFile.ReadLines())
                {
                    if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                        _bThreadAbort)
                    {
                        return;
                    }

                    var asLine = strLine.Split('\t');

                    if ((nHashColumn < asLine.Length) &&
                        (strLine.StartsWith(ksLineType_File)))
                    {
                        var fileKeyTuple =
                            FileKeyTuple
                            .FactoryCreate(asLine[nHashColumn], ("" + asLine[knColLength]).ToUlong());

                        if (null == fileKeyTuple)
                        {
                            Util.Assert(99912, false);
                            Abort();
                            return;
                        }

                       folderScore =
                            folderScore.Zip(
                            new[] { (uint)fileKeyTuple.GetHashCode() }
                            .Concat(Statics.FileDictionary.GetFolderScorer(fileKeyTuple)),
                            (n1, n2) => n1 + n2)
                            .ToArray();
                    }
                    else if (strLine.StartsWith(ksLineType_Directory))
                    {
                        dirData.AddToTree(
                            asLine[2],
                            (uint)("" + asLine[1]).ToInt(),
                            ("" + asLine[knColLength]).ToUlong(),
                            folderScore);

                        folderScore = new[] { 0U, 0U, 0U };  // Weighted folder scores: HashParity (random); largest; smallest
                    }
                }

                // 8s
                Util.WriteLine("FolderScore " + (DateTime.Now - dt).TotalMilliseconds + " ms - " + _lvItemProjectVM.Volume);

                var rootTreeNode = dirData.AddToTree();

                if (null != rootTreeNode)
                {
                    rootTreeNode.NodeDatum = new RootNodeDatum(
                        rootTreeNode.NodeDatum,
                        _lvItemProjectVM,
                        nVolFree, nVolLength);

                    TreeSubnodeDetails(rootTreeNode);
                }

                StatusCallback(_lvItemProjectVM, rootTreeNode);

#if (DEBUG && FOOBAR)
                Util.WriteLine("" + _volStrings.ListingFile.ReadLines().Where(s => s.StartsWith(ksLineType_File)).Sum(s => (decimal)(s.Split('\t')[knColLength]).ToUlong()));
                Util.WriteLine("" + _volStrings.ListingFile.ReadLines().Where(s => s.StartsWith(ksLineType_Directory)).Sum(s => (decimal)(s.Split('\t')[knColLength]).ToUlong()));

                ulong nScannedLength = 0;

                _volStrings.ListingFile
                    .ReadLines()
                    .Where(s => s.StartsWith(ksLineType_Length))
                    .FirstOnlyAssert(s => nScannedLength = (s.Split('\t')[knColLength]).ToUlong());

                Util.WriteLine("" + nScannedLength);

                ulong nTotalLength = 0;

                if (rootTreeNode != null)
                {
                    nTotalLength = ((RootNodeDatum)rootTreeNode.NodeDatum).TotalLength;
                }

                if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                    return;     // to avoid the below assert box

                if (nScannedLength != nTotalLength)
                {
                    Util.WriteLine("" + nTotalLength);
                    Util.Assert(1301.23101m, false, "nScannedLength != nTotalLength\n" + _volStrings.ListingFile, bTraceOnly: true);
                }

                Util.WriteLine(_volStrings.ListingFile + " tree took " + (DateTime.Now - dtStart).TotalMilliseconds / 1000d + " seconds.");
#endif
            }

            void
                StatusCallback(LVitem_ProjectVM volStrings, LocalTreeNode rootNode = null, bool bError = false)
            {
                var treeStatus = _callbackWR?.Get(w => w);

                if (null == treeStatus)
                {
                    Util.Assert(99866, false);
                    return;
                }

                treeStatus.Status(volStrings, rootNode, bError);
            }

            Thread
                _thread = new Thread(() => { });
            bool
                _bThreadAbort = false;
            readonly LVitem_ProjectVM
                _lvItemProjectVM = null;
        }
    }
}
