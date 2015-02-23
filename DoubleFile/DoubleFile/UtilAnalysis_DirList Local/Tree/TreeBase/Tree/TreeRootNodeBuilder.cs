using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DoubleFile;

namespace Local
{
    partial class Tree
    {
        partial class TreeRootNodeBuilder : TreeBase
        {
            internal TreeRootNodeBuilder(LVitem_ProjectVM volStrings, TreeBase base_in)
                : base(base_in)
            {
                m_volStrings = volStrings;
                MBoxStatic.Assert(1301.2301, m_statusCallback != null);
            }

            DetailsDatum TreeSubnodeDetails(LocalTreeNode treeNode)
            {
                var datum = new DetailsDatum();

                foreach (var node in treeNode.Nodes)
                {
                    if (m_bThreadAbort || gd.WindowClosed)
                    {
                        return datum;
                    }

                    datum += TreeSubnodeDetails(node);
                }

                var nodeDatum = treeNode.Tag as NodeDatum;

                if (null == nodeDatum)
                {
                    return datum;
                }

                if (nodeDatum.nLineNo == 0)
                {
                    return datum;
                }

                nodeDatum.nTotalLength = (datum.nTotalLength += nodeDatum.nLength);
                nodeDatum.nImmediateFiles = (nodeDatum.nLineNo - nodeDatum.nPrevLineNo - 1);
                nodeDatum.nFilesInSubdirs = (datum.nFilesInSubdirs += nodeDatum.nImmediateFiles);
                nodeDatum.nSubDirs = (datum.nSubDirs += (uint)treeNode.Nodes.Count);

                if (nodeDatum.nImmediateFiles > 0)
                {
                    ++datum.nDirsWithFiles;
                }

                nodeDatum.nDirsWithFiles = datum.nDirsWithFiles;

                UList<LocalTreeNode> lsTreeNodes = null;

                if (m_dictNodes.TryGetValue(nodeDatum.Key, out lsTreeNodes))
                {
                    lsTreeNodes.Add(treeNode);
                }
                else if (nodeDatum.nTotalLength > 100 * 1024)
                {
                    m_dictNodes[nodeDatum.Key] = new UList<LocalTreeNode> { treeNode };
                }

                return datum;
            }

            internal TreeRootNodeBuilder DoThreadFactory()
            {
                m_thread = new Thread(Go) { IsBackground = true };
                m_thread.Start();
                return this;
            }

            internal void Join()
            {
                m_thread.Join();
            }

            internal void Abort()
            {
                m_bThreadAbort = true;
                m_thread.Abort();
            }

            void Go()
            {
                var dtStart = DateTime.Now;

                if (m_volStrings.CanLoad == false)
                {
                    MBoxStatic.Assert(1301.2307, false);    // guaranteed by caller
                    return;
                }

                {
                    var bValid = false;
                    var bAttemptConvert = false;

                    while (true)
                    {
                        bValid = ValidateFile(m_volStrings.ListingFile);

                        if (bValid || bAttemptConvert)
                        {
                            break;
                        }

                        if (false == File.Exists(StrFile_01(m_volStrings.ListingFile)))
                        {
                            break;
                        }

                        try
                        {
                            File.Delete(StrFile_01(m_volStrings.ListingFile));
                        }
                        catch (IOException) { }

                        bAttemptConvert = true;
                    }

                    if (bValid == false)
                    {
                        MBoxStatic.ShowDialog("Bad file: " + m_volStrings.ListingFile, "Tree");
                        m_statusCallback(m_volStrings, bError: true);
                        return;
                    }
                }

                ulong nVolFree = 0;
                ulong nVolLength = 0;

                {
                    var ieDriveInfo = File
                        .ReadLines(m_volStrings.ListingFile)
                        .Where(s => s.StartsWith(ksLineType_VolumeInfo));
                    var strBuilder = new StringBuilder();
                    var nIx = -1;

                    foreach (var strArray
                        in ieDriveInfo
                        .Select(strLine => strLine.Split('\t')))
                    {
                        ++nIx;

                        if (strArray.Length > 3)
                        {
                            strBuilder.Append(strArray[2]);
                        }
                        else if (strArray.Length > 2)
                        {
                            strBuilder.Append(FileParse.kasDIlabels[nIx]);
                        }
                        else
                        {
                            continue;
                        }

                        var s = strArray[strArray.Length - 1];

                        strBuilder.AppendLine('\t' + s);

                        if ((nIx == 5) && (false == string.IsNullOrWhiteSpace(s)))
                        {
                            nVolFree = ulong.Parse(s);
                        }
                        else if ((nIx == 6) && (false == string.IsNullOrWhiteSpace(s)))
                        {
                            nVolLength = ulong.Parse(s);
                        }
                    }

                    lock (m_dictDriveInfo)
                    {
                        if (m_dictDriveInfo.ContainsKeyA(m_volStrings.ListingFile))
                        {
                            MBoxStatic.Assert(1301.2308, false);
                            m_dictDriveInfo.Remove(m_volStrings.ListingFile);
                        }

                        m_dictDriveInfo.Add(m_volStrings.ListingFile, strBuilder.ToString().Trim('\r', '\n'));
                    }
                }

                DirData dirData = null;

                {
                    var rootNode = new RootNode();
                    
                    File
                        .ReadLines(m_volStrings.ListingFile)
                        .Where(s => s.StartsWith(ksLineType_Start))
                        .FirstOnlyAssert(s => rootNode.FirstLineNo = uint.Parse(s.Split('\t')[1]));
                    dirData = new DirData(gd, rootNode);
                }

                var ieLines = File
                    .ReadLines(m_volStrings.ListingFile)
                    .Where(s => s.StartsWith(ksLineType_Directory));

                foreach (var strLine in ieLines)
                {
                    if (gd.WindowClosed)
                    {
                        return;
                    }

                    var strArray = strLine.Split('\t');

                    dirData.AddToTree(strArray[2], uint.Parse(strArray[1]), ulong.Parse(strArray[knColLength]));
                }

                var rootTreeNode = dirData.AddToTree(m_volStrings.Nickname);

                if (rootTreeNode != null)
                {
                    rootTreeNode.Tag = new RootNodeDatum((NodeDatum)rootTreeNode.Tag, 
                        m_volStrings.ListingFile, m_volStrings.VolumeGroup, nVolFree, nVolLength);
                    TreeSubnodeDetails(rootTreeNode);
                }

                m_statusCallback(m_volStrings, rootTreeNode);

#if (DEBUG && FOOBAR)
                UtilProject.WriteLine(File.ReadLines(m_volStrings.ListingFile).Where(s => s.StartsWith(ksLineType_File)).Sum(s => double.Parse(s.Split('\t')[knColLength])).ToString());
                UtilProject.WriteLine(File.ReadLines(m_volStrings.ListingFile).Where(s => s.StartsWith(ksLineType_Directory)).Sum(s => double.Parse(s.Split('\t')[knColLength])).ToString());

                ulong nScannedLength = 0;

                File
                    .ReadLines(m_volStrings.ListingFile)
                    .Where(s => s.StartsWith(ksLineType_Length))
                    .FirstOnlyAssert(s => nScannedLength = ulong.Parse(s.Split('\t')[knColLength]));

                UtilProject.WriteLine(nScannedLength.ToString());

                ulong nTotalLength = 0;

                if (rootTreeNode != null)
                {
                    nTotalLength = ((RootNodeDatum)rootTreeNode.Tag).nTotalLength;
                }

                if (gd.WindowClosed)
                {
                    return;     // to avoid the below assert box
                }

                if (nScannedLength != nTotalLength)
                {
                    UtilProject.WriteLine(nTotalLength.ToString());
                    MBoxStatic.Assert(1301.23101, false, "nScannedLength != nTotalLength\n" + m_volStrings.ListingFile, bTraceOnly: true);
                }

                UtilProject.WriteLine(m_volStrings.ListingFile + " tree took " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
#endif
            }

            Thread m_thread = null;
            bool m_bThreadAbort = false;
            readonly LVitem_ProjectVM m_volStrings = null;
        }
    }
}
