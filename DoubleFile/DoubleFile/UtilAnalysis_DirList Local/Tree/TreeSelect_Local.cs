﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DoubleFile;

namespace Local
{
    partial class TreeSelect
    {
        static internal event Action<IEnumerable<string>, string> FileListUpdated;
        static internal event Action<IEnumerable<string[]>, LocalTreeNode> FolderDetailUpdated;
        static internal event Action<IEnumerable<string[]>, string> VolumeDetailUpdated;

        void Go()
        {
            if (null != FileListUpdated)
                GetFileList();

            if (null != FolderDetailUpdated)
                GetFolderDetail();

            if (null != VolumeDetailUpdated)
                GetVolumeDetail();
        }

        // The main file has the original GetFileList() that returns multiple info columns
        // That one is static and is used by the treemap.
        // However it deprecatedly splits each line and formats it:
        // unused and too far upstream
        void GetFileList()
        {
            string strListingFile = null;
            var nodeDatum = _treeNode.Tag as NodeDatum;
            var rootNodeDatum = _treeNode.Root().Tag as RootNodeDatum;
            IEnumerable<string> lsFiles = null;

            do
            {
                if ((null == nodeDatum) ||
                    (0 == nodeDatum.nLineNo) ||
                    (null == rootNodeDatum))
                {
                    break;
                }

                strListingFile = rootNodeDatum.StrFile;

                var nPrevDir = (int)nodeDatum.nPrevLineNo;
                var nLineNo = (int)nodeDatum.nLineNo;

                if (0 == nPrevDir)
                    break;

                if (1 >= (nLineNo - nPrevDir))  // dir has no files
                    break;

                lsFiles =
                    File
                    .ReadLines(strListingFile)
                    .Skip(nPrevDir)
                    .Take((nLineNo - nPrevDir - 1))
                    .ToList();
            } while (false);

            FileListUpdated(lsFiles, strListingFile);
        }

        void GetFolderDetail()
        {
            var nodeDatum = _treeNode.Tag as NodeDatum;
            var lasItems = new List<string[]>();

            if ((null == nodeDatum) ||
                (0 == nodeDatum.nLineNo))
            {
                FolderDetailUpdated(lasItems, null);
                return;
            }

            const string NUMFMT = "###,###,###,##0";

            lasItems.Add(new[] { "# Immediate Folders", _treeNode.Nodes.Count.ToString(NUMFMT) });
            lasItems.Add(new[] { "Total # Files", nodeDatum.nFilesInSubdirs.ToString(NUMFMT) });

            if (nodeDatum.nSubDirs > 0)
            {
                var strItem = nodeDatum.nSubDirs.ToString(NUMFMT);

                if (nodeDatum.nDirsWithFiles > 0)
                {
                    var nDirsWithFiles = nodeDatum.nDirsWithFiles;

                    if (nodeDatum.nImmediateFiles > 0)
                    {
                        --nDirsWithFiles;
                    }

                    if (nDirsWithFiles > 0)
                    {
                        strItem += " (" + nDirsWithFiles.ToString(NUMFMT) + " with files)";
                    }
                }

                lasItems.Add(new string[] { "# Subfolders", strItem });
            }

            lasItems.Add(new string[] { "Total Size", FormatSize(nodeDatum.nTotalLength, bBytes: true) });
            FolderDetailUpdated(lasItems, _treeNode);
        }

        void GetVolumeDetail()
        {
            string strDriveInfo = null;

            var rootNode = _treeNode;

            while (null != rootNode.Parent)
                rootNode = rootNode.Parent;

            if (_dictVolumeInfo.TryGetValue(((RootNodeDatum)rootNode.Tag).StrFile, out strDriveInfo))
            {
                var arrDriveInfo =
                    strDriveInfo
                    .Split(new[] { "\r\n", "\n" },
                    StringSplitOptions.None);

                MBoxStatic.Assert(1301.2314,
                    new[] { 7, 8, 10, kanDIviewOrder.Length }
                    .Contains(arrDriveInfo.Length));

                var asItems = new string[arrDriveInfo.Length][];

                for (var i = 0; i < arrDriveInfo.Length; ++i)
                {
                    var a = arrDriveInfo[i].Split('\t');

                    if (0 == a[1].Trim().Length)
                    {
                        continue;
                    }

                    asItems[i] = new[]
                    {
                        a[0],
                        kabDIsizeType[i]
                            ? FormatSize(a[1], bBytes: true) 
                            : a[1]
                    };
                }

                var lasItems = new List<string[]>();

                for (var ix = 0; ix < arrDriveInfo.Length; ++ix)
                {
                    if ((null == asItems[ix]) ||
                        (0 == asItems[ix].Length) ||
                        (0 == asItems[ix][1].Trim().Length))
                    {
                        continue;
                    }

                    if ((kanDIoptIfEqTo[ix] != -1) &&
                        (asItems[ix][1] == asItems[kanDIoptIfEqTo[ix]][1]))
                    {
                        continue;
                    }

                    var ixA = (arrDriveInfo.Length == kanDIviewOrder.Length)
                        ? kanDIviewOrder[ix]
                        : ix;

                    lasItems.Add(asItems[ix]);
                }

                VolumeDetailUpdated(lasItems.Where(i => i != null), rootNode.Text);
            }
        }
    }
}
