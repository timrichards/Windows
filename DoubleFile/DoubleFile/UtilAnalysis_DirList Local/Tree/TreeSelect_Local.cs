using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DoubleFile;

namespace Local
{
    delegate void TreeSelectDelegate_FileList(IEnumerable<string> lsFiles, string strListingFile);
    delegate void TreeSelectDelegate_FolderDetail(IEnumerable<string[]> lsDetail);
    delegate void TreeSelectDelegate_VolumeDetail(IEnumerable<string[]> lsDetail);
    
    partial class TreeSelect
    {
        void Go()
        {
            GetFileList();
            GetFolderDetail();
            GetVolumeDetail();
        }

        // There is an older GetFileList() that returns multiple info columns
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

            _callback_1_FileList(lsFiles, strListingFile);
        }

        void GetFolderDetail()
        {
            var nodeDatum = _treeNode.Tag as NodeDatum;
            var lasItems = new List<string[]>();

            if ((null == nodeDatum) ||
                (0 == nodeDatum.nLineNo))
            {
                _callback_2_FolderDetail(lasItems);
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
            _callback_2_FolderDetail(lasItems);
        }

        void GetVolumeDetail()
        {
            string strDriveInfo = null;

            if (_dictVolumeInfo.TryGetValue(_strFile, out strDriveInfo))
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

                    if (a[1].Trim().Length == 0)
                    {
                        continue;
                    }

                    asItems[i] = new[]
                    {
                        a[0],
                        kabDIsizeType[i] ?
                            FormatSize(a[1], bBytes: true) :
                            a[1]
                    };
                }

                var lasItems = new List<string[]>();

                for (var ix = 0; ix < arrDriveInfo.Length; ++ix)
                {
                    if ((asItems[ix] == null) ||
                        (asItems[ix].Length == 0) ||
                        (asItems[ix][1].Trim().Length == 0))
                    {
                        continue;
                    }

                    if ((kanDIoptIfEqTo[ix] != -1) &&
                        (asItems[ix][1] == asItems[kanDIoptIfEqTo[ix]][1]))
                    {
                        continue;
                    }

                    var ixA = (arrDriveInfo.Length == kanDIviewOrder.Length) ?
                        kanDIviewOrder[ix] :
                        ix;

                    lasItems.Add(asItems[ix]);
                }

                _callback_3_VolumeDetail(lasItems.Where(i => i != null));
            }
        }

        readonly TreeSelectDelegate_FileList
            _callback_1_FileList = null;
        readonly TreeSelectDelegate_FolderDetail
            _callback_2_FolderDetail;
        readonly TreeSelectDelegate_VolumeDetail
            _callback_3_VolumeDetail;
    }
}
