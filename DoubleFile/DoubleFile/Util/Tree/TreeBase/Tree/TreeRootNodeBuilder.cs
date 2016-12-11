using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Windows;
using System.IO.IsolatedStorage;
using System.Diagnostics;

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
                TreeRootNodeBuilder(LVitemProject_Explorer lvItemProjectVM, TreeBase base_in)
                : base(base_in)
            {
                _lvItemProjectVM = lvItemProjectVM;
                Util.Assert(1301.2301m, _callbackWR != null);
            }

            NodeDatum
                TreeSubnodeDetails(LocalTreeNode treeNode)
            {
                var datum = new NodeDatum();

                if (null != treeNode.Nodes)
                {
                    foreach (var node in treeNode.Nodes)
                        datum.AddDatum(TreeSubnodeDetails(node));       // recurse
                }

                treeNode.NodeDatum.SetDatum(datum, (uint)(treeNode.Nodes?.Count ?? 0));
                return datum;
            }

            internal void
                Go(CancellationTokenSource cts)
            {
                _cts = cts;

                var stopwatch = Stopwatch.StartNew();

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
                        MBoxStatic.ShowOverlay("Bad file: " + _lvItemProjectVM.ListingFile, "Tree");
                        StatusCallback(_lvItemProjectVM, bError: true);
                        return;
                    }
                }

                ulong nVolFree = 0;
                ulong nVolLength = 0;

                {
                    var ieDriveInfo =
                        _lvItemProjectVM.ListingFile
                        .ReadLines(99642)
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

                RootNode rootNode = null;

                {
                    var bHit = false;

                    var asLines =
                        _lvItemProjectVM.ListingFile
                        .ReadLines(99641)
                        .TakeWhile(s =>
                        {
                            if (bHit)
                                return false;

                            bHit = s.StartsWith(ksLineType_Start);
                            return true;
                        })
                        .ToArray();

                    var driveLetter =
                        asLines
                        .Where(s => s.StartsWith(ksLineType_Path))
                        .Select(s => s.Split('\t')[2][0])
                        .FirstOnlyAssert();

                    var nFirstLineNo =
                        asLines
                        .Where(s => s.StartsWith(ksLineType_Start))
                        .Select(s => (uint)s.Split('\t')[1].ToInt())
                        .FirstOnlyAssert();

                    rootNode = new RootNode(nFirstLineNo, driveLetter);
                }

                var lsFilesHereIndexedIDs = new List<int> { };
                var isHashComplete = true;
                var nHashColumn = Statics.DupeFileDictionary.HashColumn;

                foreach (var strLine in _lvItemProjectVM.ListingFile.ReadLines(99640))
                {
                    if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                        _cts.IsCancellationRequested)
                    {
                        return;
                    }

                    var asLine = strLine.Split('\t');

                    if (strLine.StartsWith(ksLineType_File))
                    {
                        var nFileLength = ("" + asLine[knColLength]).ToUlong();

                        if (0 < nFileLength)
                        {
                            if (nHashColumn < asLine.Length)
                                lsFilesHereIndexedIDs.Add(HashTuple.FileIndexedIDfromString(asLine[nHashColumn], nFileLength));
                            else
                                isHashComplete = false;
                        }
                    }
                    else if (strLine.StartsWith(ksLineType_Directory))
                    {
                        rootNode.AddToTree(asLine, lsFilesHereIndexedIDs.OrderBy(n => n).Distinct().ToArray(), isHashComplete);
                        lsFilesHereIndexedIDs = new List<int> { };
                        isHashComplete = true;
                    }
                }

                var rootTreeNode = rootNode.AddToTree();

                if (null != rootTreeNode)
                {
                    rootTreeNode.NodeDatum = new RootNodeDatum(
                        rootTreeNode.NodeDatum,
                        _lvItemProjectVM.SetCulledPath(
                            rootTreeNode.NodeDatum.As<RootNodeDatum>()
                            ?.LVitemProjectVM.CulledPath),
                        nVolFree, nVolLength);

                    TreeSubnodeDetails(rootTreeNode);
                }

                stopwatch.Stop();       // 8s
                Util.WriteLine("TreeRootNodeBuilder " + stopwatch.ElapsedMilliseconds + " ms - " + _lvItemProjectVM.SourcePath);
                StatusCallback(_lvItemProjectVM, rootTreeNode);

#if (DEBUG && FOOBAR)
                stopwatch.Start();
                Util.WriteLine("" + _lvItemProjectVM.ListingFile.ReadLines(99787).Where(s => s.StartsWith(ksLineType_File)).Sum(s => (decimal)(s.Split('\t')[knColLength]).ToUlong()));
                Util.WriteLine("" + _lvItemProjectVM.ListingFile.ReadLines(99728).Where(s => s.StartsWith(ksLineType_Directory)).Sum(s => (decimal)(s.Split('\t')[knColLength]).ToUlong()));

                ulong nScannedLength = 0;

                _lvItemProjectVM.ListingFile
                    .ReadLines(99639)
                    .Where(s => s.StartsWith(ksLineType_Length))
                    .FirstOnlyAssert(s => nScannedLength = (s.Split('\t')[knColLength]).ToUlong());

                Util.WriteLine("" + nScannedLength);

                ulong nTotalLength = 0;

                if (rootTreeNode != null)
                {
                    nTotalLength = rootTreeNode.RootNodeDatum.LengthTotal;
                }

                if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                    return;     // to avoid the below assert box

                if (nScannedLength != nTotalLength)
                {
                    Util.WriteLine("" + nTotalLength);
                    Util.Assert(1301.23101m, false, "nScannedLength != nTotalLength\n" + _lvItemProjectVM.ListingFile, bIfDefDebug: true);
                }

                Util.WriteLine(_lvItemProjectVM.ListingFile + " tree took " + stopwatch.ElapsedMilliseconds / 1000d + " seconds.");
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

            readonly LVitemProject_Explorer
                _lvItemProjectVM = null;
            CancellationTokenSource
                _cts = null;
        }
    }
}
