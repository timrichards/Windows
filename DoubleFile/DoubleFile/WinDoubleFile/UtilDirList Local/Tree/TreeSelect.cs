using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class TreeSelect : UtilDirList
    {
        static internal IObservable<Tuple<IEnumerable<string>, string, LocalTreeNode>>
            FileListUpdated { get { return _fileListUpdated.AsObservable(); } }
        static readonly Subject<Tuple<IEnumerable<string>, string, LocalTreeNode>> _fileListUpdated = new Subject<Tuple<IEnumerable<string>, string, LocalTreeNode>>();

        static internal IObservable<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>>
            FolderDetailUpdated { get { return _folderDetailUpdated.AsObservable(); } }
        static readonly Subject<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>> _folderDetailUpdated = new Subject<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>>();

        static internal IObservable<Tuple<IEnumerable<IEnumerable<string>>, string>>
            VolumeDetailUpdated { get { return _volumeDetailUpdated.AsObservable(); } }
        static readonly Subject<Tuple<IEnumerable<IEnumerable<string>>, string>> _volumeDetailUpdated = new Subject<Tuple<IEnumerable<IEnumerable<string>>, string>>();

        static internal bool DoThreadFactory(LocalTreeNode treeNode,
            bool bCompareMode = false, bool bSecondComparePane = false)
        {
            if ((null != _thread) &&
                (_thread.IsAlive))
            {
                return false;
            }

            if (treeNode is LocalTreeMapFileNode)     // does not support immediate file fake nodes
                return false;

            if (null != WinDoubleFile_FoldersVM.LocalTV)
                _dictVolumeInfo = WinDoubleFile_FoldersVM.LocalTV._dictVolumeInfo;

            _bCompareMode = bCompareMode;
            _bSecondComparePane = bSecondComparePane;
            _thread = new Thread(() => Go(treeNode)) { IsBackground = true };
            _thread.Start();
            return true;
        }

        static void Go(LocalTreeNode treeNode)
        {
            if (null != FileListUpdated)
                GetFileList(treeNode);

            if (null != FolderDetailUpdated)
                GetFolderDetail(treeNode);

            if (null != VolumeDetailUpdated)
                GetVolumeDetail(treeNode);

            _thread = null;
        }

        static void GetFileList(LocalTreeNode treeNode)
        {
            string strListingFile = null;
            var nodeDatum = treeNode.NodeDatum;
            var rootNodeDatum = treeNode.Root().NodeDatum as RootNodeDatum;
            IEnumerable<string> lsFiles = null;

            do
            {
                if ((null == nodeDatum) ||
                    (0 == nodeDatum.LineNo) ||
                    (null == rootNodeDatum))
                {
                    break;
                }

                strListingFile = rootNodeDatum.ListingFile;

                var nPrevDir = (int)nodeDatum.PrevLineNo;
                var nLineNo = (int)nodeDatum.LineNo;

                if (0 == nPrevDir)
                    break;

                if (1 >= (nLineNo - nPrevDir))  // dir has no files
                    break;

                lsFiles =
                    File
                    .ReadLines(strListingFile)
                    .Skip(nPrevDir)
                    .Take((nLineNo - nPrevDir - 1));
            } while (false);

            _fileListUpdated.OnNext(Tuple.Create(lsFiles, strListingFile, treeNode));
        }

        static void GetFolderDetail(LocalTreeNode treeNode)
        {
            var nodeDatum = treeNode.NodeDatum;
            var lasItems = new List<IEnumerable<string>>();

            if ((null == nodeDatum) ||
                (0 == nodeDatum.LineNo))
            {
                _folderDetailUpdated.OnNext(Tuple.Create(lasItems.AsEnumerable(), (LocalTreeNode)null));
                return;
            }

            const string kStrFmt_thous = "###,###,###,##0";

            lasItems.Add(new[] { "# Files Here", nodeDatum.ImmediateFiles.ToString(kStrFmt_thous) });
            lasItems.Add(new[] { "with Size of", FormatSize(nodeDatum.Length, bBytes: true) });
            lasItems.Add(new[] { "Total # Files", nodeDatum.FilesInSubdirs.ToString(kStrFmt_thous) });
            lasItems.Add(new[] { "# Folders Here", ((null != treeNode.Nodes) ? treeNode.Nodes.Length : 0).ToString(kStrFmt_thous) });

            if (nodeDatum.SubDirs > 0)
            {
                var strItem = nodeDatum.SubDirs.ToString(kStrFmt_thous);

                if (nodeDatum.DirsWithFiles > 0)
                {
                    var nDirsWithFiles = nodeDatum.DirsWithFiles;

                    if (nodeDatum.ImmediateFiles > 0)
                    {
                        --nDirsWithFiles;
                    }

                    if (nDirsWithFiles > 0)
                    {
                        strItem += " (" + nDirsWithFiles.ToString(kStrFmt_thous) + " with files)";
                    }
                }

                lasItems.Add(new string[] { "# Subfolders", strItem });
            }

            lasItems.Add(new[] { "Total Size", FormatSize(nodeDatum.TotalLength, bBytes: true) });
            _folderDetailUpdated.OnNext(Tuple.Create(lasItems.AsEnumerable(), treeNode));
        }

        static void GetVolumeDetail(LocalTreeNode treeNode)
        {
            string strDriveInfo = null;

            var rootNode = treeNode;

            while (null != rootNode.Parent)
                rootNode = rootNode.Parent;

            if ((null == _dictVolumeInfo) ||
                (false == _dictVolumeInfo.TryGetValue(((RootNodeDatum)rootNode.NodeDatum).ListingFile, out strDriveInfo)))
            {
                _volumeDetailUpdated.OnNext(Tuple.Create((IEnumerable<IEnumerable<string>>)null, rootNode.Text));
                return;
            }

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

            var lasItems = new List<IEnumerable<string>>();

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

            _volumeDetailUpdated.OnNext(Tuple.Create(lasItems.Where(i => i != null), rootNode.Text));
        }

        static Dictionary<string, string>
            _dictVolumeInfo = null;
        static bool
            _bCompareMode = false;
        static bool
            _bSecondComparePane = false;
        static Thread
            _thread = null;
    }
}
