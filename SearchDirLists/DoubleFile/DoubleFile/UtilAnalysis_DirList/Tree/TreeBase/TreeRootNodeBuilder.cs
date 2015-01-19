﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class TreeRootNodeBuilder : TreeBase
    {
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        LVitem_ProjectVM m_volStrings = null;

        internal TreeRootNodeBuilder(LVitem_ProjectVM volStrings, TreeBase base_in)
            : base(base_in)
        {
            m_volStrings = volStrings;
            MBox.Assert(1301.2301, m_statusCallback != null);
        }

        DetailsDatum TreeSubnodeDetails(TreeNode treeNode)
        {
            DetailsDatum datum = new DetailsDatum();

            foreach (TreeNode node in treeNode.Nodes)
            {
                if (m_bThreadAbort || GlobalData.AppExit)
                {
                    return datum;
                }

                datum += TreeSubnodeDetails(node);
            }

            if ((treeNode.Tag is NodeDatum) == false)
            {
                return datum;
            }

            NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

            if (nodeDatum.nLineNo <= 0)
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

            Correlate nKey = nodeDatum.Key;

            lock (m_dictNodes)
            {
                if (m_dictNodes.ContainsKey(nKey))
                {
                    m_dictNodes[nKey].Add(treeNode);
                }
                else if (nodeDatum.nTotalLength > 100 * 1024)
                {
                    UList<TreeNode> listNodes = new UList<TreeNode>();

                    listNodes.Add(treeNode);
                    m_dictNodes.Add(nKey, listNodes);
                }
            }

            return datum;
        }

        internal void Go()
        {
            DateTime dtStart = DateTime.Now;

            if (m_volStrings.CanLoad == false)
            {
                MBox.Assert(1301.2307, false);    // guaranteed by caller
                return;
            }

            string strVolumeName = m_volStrings.VolumeName;
            string strPath = m_volStrings.StrPath;
            string strSaveAs = m_volStrings.SaveAs;

            if (FormatPath(ref strPath, ref strSaveAs, false) == false)
            {
                return;
            }

            {
                bool bValid = false;
                bool bAttemptConvert = false;

                while (true)
                {
                    bValid = ValidateFile(strSaveAs);

                    if (bValid || bAttemptConvert)
                    {
                        break;
                    }

                    if (File.Exists(StrFile_01(strSaveAs)) == false)
                    {
                        break;
                    }

                    try
                    {
                        File.Delete(StrFile_01(strSaveAs));
                    }
                    catch { }

                    bAttemptConvert = true;
                }

                if (bValid == false)
                {
                    MBox("Bad file: " + strSaveAs, "Tree");
                    m_statusCallback(m_volStrings, bError: true);
                    return;
                }
            }

            ulong nVolFree = 0;
            ulong nVolLength = 0;

            {
                string[] arrDriveInfo = File.ReadLines(strSaveAs).Where(s => s.StartsWith(ksLineType_DriveInfo)).ToArray();
                StringBuilder strBuilder = new StringBuilder();
                int nIx = -1;

                foreach (string strLine in arrDriveInfo)
                {
                    string[] strArray = strLine.Split('\t');
                    ++nIx;

                    if (strArray.Length > 3)
                    {
                        strBuilder.Append(strArray[2]);
                    }
                    else if (strArray.Length > 2)
                    {
                        strBuilder.Append(Utilities.mAstrDIlabels[nIx]);
                    }
                    else
                    {
                        continue;
                    }

                    string s = strArray[strArray.Length - 1];
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
                    if (m_dictDriveInfo.ContainsKey(strSaveAs))
                    {
                        MBox.Assert(1301.2308, false);
                        m_dictDriveInfo.Remove(strSaveAs);
                    }

                    m_dictDriveInfo.Add(strSaveAs, strBuilder.ToString().Trim(new char[] { '\r', '\n' }));
                }
            }

            DirData dirData = null;

            {
                RootNode rootNode = new RootNode();
                string strStart = File.ReadLines(strSaveAs).Where(s => s.StartsWith(ksLineType_Start)).ToArray()[0];

                rootNode.FirstLineNo = uint.Parse(strStart.Split('\t')[1]);
                dirData = new DirData(rootNode);
            }

            bool bZeroLengthsWritten = true;
            List<string> listLines = File.ReadLines(strSaveAs).Where(s => s.StartsWith(ksLineType_Directory)).ToList();

            foreach (string strLine in listLines)
            {
                if (GlobalData.AppExit)
                {
                    return;
                }

                string[] strArray = strLine.Split('\t');
                uint nLineNo = uint.Parse(strArray[1]);
                int nIx = knColLength;
                ulong nLength = 0;

                if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx])))
                {
                    nLength = ulong.Parse(strArray[nIx]);
                }
                else
                {
                    bZeroLengthsWritten = false;     // files created before 140509 Fri drop zeroes from the end of the line
                }

                string strDir = strArray[2];

                dirData.AddToTree(strDir, nLineNo, nLength);
            }

            TreeNode rootTreeNode = dirData.AddToTree(strVolumeName);

            if (rootTreeNode != null)
            {
                rootTreeNode.Tag = new RootNodeDatum((NodeDatum)rootTreeNode.Tag, strSaveAs, m_volStrings.VolumeGroup, nVolFree, nVolLength);
                TreeSubnodeDetails(rootTreeNode);
            }

            m_statusCallback(m_volStrings, rootTreeNode);

            if (bZeroLengthsWritten)
            {
#if (DEBUG)
                UtilProject.WriteLine(File.ReadLines(strSaveAs).Where(s => s.StartsWith(ksLineType_File)).Sum(s => decimal.Parse(s.Split('\t')[knColLength])).ToString());
                UtilProject.WriteLine(File.ReadLines(strSaveAs).Where(s => s.StartsWith(ksLineType_Directory)).Sum(s => decimal.Parse(s.Split('\t')[knColLength])).ToString());
#endif
            }

            ulong nScannedLength = ulong.Parse(
                File.ReadLines(strSaveAs).Where(s => s.StartsWith(ksLineType_Length)).ToArray()[0]
                .Split('\t')[knColLength]);

            UtilProject.WriteLine(nScannedLength.ToString());

            ulong nTotalLength = 0;

            if (rootTreeNode != null)
            {
                nTotalLength = ((RootNodeDatum)rootTreeNode.Tag).nTotalLength;
            }

            if (nScannedLength != nTotalLength)
            {
                UtilProject.WriteLine(nTotalLength.ToString());
                MBox.Assert(1301.23101, false, "nScannedLength != nTotalLength\n" + strSaveAs, bTraceOnly: true);
            }

            UtilProject.WriteLine(strSaveAs + " tree took " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
        }

        internal TreeRootNodeBuilder DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
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
    }
}