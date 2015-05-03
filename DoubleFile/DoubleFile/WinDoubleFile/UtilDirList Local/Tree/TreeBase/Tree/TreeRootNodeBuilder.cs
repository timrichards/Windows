using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Reactive.Subjects;

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
            internal TreeRootNodeBuilder(LVitem_ProjectVM volStrings, TreeBase base_in)
                : base(base_in)
            {
                _volStrings = volStrings;
                MBoxStatic.Assert(1301.2301, _callbackWR != null);
            }

            DetailsDatum TreeSubnodeDetails(LocalTreeNode treeNode)
            {
                var datum = new DetailsDatum();

                if (null != treeNode.Nodes)
                {
                    foreach (var node in treeNode.Nodes)
                    {
                        if (App.LocalExit || _bThreadAbort)
                            return datum;

                        datum += TreeSubnodeDetails(node);
                    }
                }

                var nodeDatum = treeNode.NodeDatum;

                if ((null == nodeDatum) ||
                    (0 == nodeDatum.LineNo))
                {
                    return datum;
                }

                nodeDatum.TotalLength = (datum.TotalLength += nodeDatum.Length);
                nodeDatum.ImmediateFiles = nodeDatum.LineNo - nodeDatum.PrevLineNo - 1;
                nodeDatum.FilesInSubdirs = (datum.FilesInSubdirs += nodeDatum.ImmediateFiles);
                nodeDatum.SubDirs = (datum.SubDirs += (null != treeNode.Nodes) ? (uint)treeNode.Nodes.Length : 0);
                nodeDatum.HashParity = (datum.HashParity += nodeDatum.HashParity);

                if (0 < nodeDatum.ImmediateFiles)
                    ++datum.DirsWithFiles;

                nodeDatum.DirsWithFiles = datum.DirsWithFiles;

                List<LocalTreeNode> lsTreeNodes = null;

                if (_dictNodes.TryGetValue(nodeDatum.Key, out lsTreeNodes))
                {
                    lock (lsTreeNodes)
                        lsTreeNodes.Add(treeNode);
                }
                else if (0 < nodeDatum.TotalLength)
                    _dictNodes[nodeDatum.Key] = new List<LocalTreeNode> { treeNode };

                return datum;
            }

            internal TreeRootNodeBuilder DoThreadFactory()
            {
                _thread = new Thread(Go) { IsBackground = true };
                _thread.Start();
                return this;
            }

            internal void Join()
            {
                _thread.Join();
            }

            internal void Abort()
            {
                _bThreadAbort = true;
                _thread.Abort();
            }

            void Go()
            {
                var dtStart = DateTime.Now;

                if (_volStrings.CanLoad == false)
                {
                    MBoxStatic.Assert(1301.2307, false);    // guaranteed by caller
                    return;
                }

                {
                    var bValid = false;
                    var bAttemptConvert = false;

                    while (true)
                    {
                        bValid = ValidateFile(_volStrings.ListingFile);

                        if (bValid || bAttemptConvert)
                            break;

                        if (false == File.Exists(StrFile_01(_volStrings.ListingFile)))
                            break;

                        try
                        {
                            File.Delete(StrFile_01(_volStrings.ListingFile));
                        }
                        catch (IOException) { }

                        bAttemptConvert = true;
                    }

                    if (false == bValid)
                    {
                        MBoxStatic.ShowDialog("Bad file: " + _volStrings.ListingFile, "Tree");
                        StatusCallback(_volStrings, bError: true);
                        return;
                    }
                }

                ulong nVolFree = 0;
                ulong nVolLength = 0;

                {
                    var ieDriveInfo =
                        File
                        .ReadLines(_volStrings.ListingFile)
                        .Where(s => s.StartsWith(ksLineType_VolumeInfo));

                    var strBuilder = new StringBuilder();
                    var nIx = -1;

                    foreach (var strArray
                        in ieDriveInfo
                        .Select(strLine => strLine.Split('\t')))
                    {
                        ++nIx;

                        if (3 < strArray.Length)
                            strBuilder.Append(strArray[2]);
                        else if (2 < strArray.Length)
                            strBuilder.Append(FileParse.kasDIlabels[nIx]);
                        else
                            continue;

                        var s = strArray[strArray.Length - 1];

                        strBuilder.AppendLine('\t' + s);

                        if ((5 == nIx) &&
                            (false == string.IsNullOrWhiteSpace(s)))
                        {
                            nVolFree = ulong.Parse(s);
                        }
                        else if ((6 == nIx) &&
                            (false == string.IsNullOrWhiteSpace(s)))
                        {
                            nVolLength = ulong.Parse(s);
                        }
                    }

                    lock (_dictDriveInfo)
                    {
                        if (_dictDriveInfo.ContainsKeyA(_volStrings.ListingFile))
                        {
                            MBoxStatic.Assert(1301.2308, false);
                            _dictDriveInfo.Remove(_volStrings.ListingFile);
                        }

                        _dictDriveInfo.Add(_volStrings.ListingFile, ("" + strBuilder).Trim('\r', '\n'));
                    }
                }

                DirData dirData = null;

                {
                    var rootNode = new RootNode();
                    
                    File
                        .ReadLines(_volStrings.ListingFile)
                        .Where(s => s.StartsWith(ksLineType_Start))
                        .FirstOnlyAssert(s => rootNode.FirstLineNo = uint.Parse(s.Split('\t')[1]));

                    dirData = new DirData(rootNode);
                }

                var ieLines =
                    File
                    .ReadLines(_volStrings.ListingFile);

                var nHashParity = 0;

                foreach (var strLine in ieLines)
                {
                    if (App.LocalExit || _bThreadAbort)
                        return;

                    var asLine = strLine.Split('\t');

                    if ((10 < asLine.Length) &&
                        (strLine.StartsWith(ksLineType_File)))
                    {
                        nHashParity += new FileKeyTuple(asLine[10], asLine[knColLength]).GetHashCode();
                    }
                    else if (strLine.StartsWith(ksLineType_Directory))
                    {
                        dirData.AddToTree(asLine[2], uint.Parse(asLine[1]), ulong.Parse(asLine[knColLength]),
                            nHashParity);
                        nHashParity = 0;
                    }
                }

                string strRootPath = null;
                var rootTreeNode = dirData.AddToTree(_volStrings.Nickname, out strRootPath);

                if (null != rootTreeNode)
                {
                    rootTreeNode.NodeDatum = new RootNodeDatum(
                        rootTreeNode.NodeDatum,
                        _volStrings.ListingFile, _volStrings.VolumeGroup,
                        nVolFree, nVolLength,
                        strRootPath
                    );

                    TreeSubnodeDetails(rootTreeNode);
                }

                StatusCallback(_volStrings, rootTreeNode);

#if (DEBUG && FOOBAR)
                UtilProject.WriteLine("" + File.ReadLines(_volStrings.ListingFile).Where(s => s.StartsWith(ksLineType_File)).Sum(s => double.Parse(s.Split('\t')[knColLength])));
                UtilProject.WriteLine("" + File.ReadLines(_volStrings.ListingFile).Where(s => s.StartsWith(ksLineType_Directory)).Sum(s => double.Parse(s.Split('\t')[knColLength])));

                ulong nScannedLength = 0;

                File
                    .ReadLines(_volStrings.ListingFile)
                    .Where(s => s.StartsWith(ksLineType_Length))
                    .FirstOnlyAssert(s => nScannedLength = ulong.Parse(s.Split('\t')[knColLength]));

                UtilProject.WriteLine("" + nScannedLength);

                ulong nTotalLength = 0;

                if (rootTreeNode != null)
                {
                    nTotalLength = ((RootNodeDatum)rootTreeNode.NodeDatum).TotalLength;
                }

                if (App.LocalExit)
                    return;     // to avoid the below assert box

                if (nScannedLength != nTotalLength)
                {
                    UtilProject.WriteLine("" + nTotalLength);
                    MBoxStatic.Assert(1301.23101, false, "nScannedLength != nTotalLength\n" + _volStrings.ListingFile, bTraceOnly: true);
                }

                UtilProject.WriteLine(_volStrings.ListingFile + " tree took " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
#endif
            }

            void StatusCallback(LVitem_ProjectVM volStrings, LocalTreeNode rootNode = null, bool bError = false)
            {
                if (null == _callbackWR)
                {
                    MBoxStatic.Assert(99867, false);
                    return;
                }

                ITreeStatus treeStatus = null;

                _callbackWR.TryGetTarget(out treeStatus);

                if (null == treeStatus)
                {
                    MBoxStatic.Assert(99866, false);
                    return;
                }

                treeStatus.Status(volStrings, rootNode, bError);
            }

            Thread
                _thread = null;
            bool
                _bThreadAbort = false;
            readonly LVitem_ProjectVM
                _volStrings = null;
        }
    }
}
