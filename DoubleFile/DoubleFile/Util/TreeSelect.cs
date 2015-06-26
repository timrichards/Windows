using System.Collections.Generic;
using System.Threading;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class TreeSelect : Util
    {
        static internal IObservable<Tuple<Tuple<IEnumerable<string>, string, LocalTreeNode, string>, int>>
            FileListUpdated { get { return _fileListUpdated.AsObservable(); } }
        static readonly LocalSubject<Tuple<IEnumerable<string>, string, LocalTreeNode, string>> _fileListUpdated = new LocalSubject<Tuple<IEnumerable<string>, string, LocalTreeNode, string>>();
        static void FileListUpdatedOnNext(Tuple<IEnumerable<string>, string, LocalTreeNode, string> value, int nInitiator) { _fileListUpdated.LocalOnNext(value, 99846, nInitiator); }

        static internal IObservable<Tuple<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>, int>>
            FolderDetailUpdated { get { return _folderDetailUpdated.AsObservable(); } }
        static readonly LocalSubject<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>> _folderDetailUpdated = new LocalSubject<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>>();
        static void FolderDetailUpdatedOnNext(Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode> value, int nInitiator) { _folderDetailUpdated.LocalOnNext(value, 99845, nInitiator); }

        static internal IObservable<Tuple<Tuple<IEnumerable<IEnumerable<string>>, string>, int>>
            VolumeDetailUpdated { get { return _volumeDetailUpdated.AsObservable(); } }
        static readonly LocalSubject<Tuple<IEnumerable<IEnumerable<string>>, string>> _volumeDetailUpdated = new LocalSubject<Tuple<IEnumerable<IEnumerable<string>>, string>>();
        static void VolumeDetailUpdatedOnNext(Tuple<IEnumerable<IEnumerable<string>>, string> value, int nInitiator) { _volumeDetailUpdated.LocalOnNext(value, 99844, nInitiator); }

        static internal bool DoThreadFactory(LocalTreeNode treeNode, int nInitiator, string strFile = null,
            bool bCompareMode = false, bool bSecondComparePane = false)
        {
            if ((null != _thread) &&
                (_thread.IsAlive))
            {
                return false;
            }

            if (treeNode is LocalTreeMapFileNode)     // does not support file fake nodes
                return false;

            _dictVolumeInfo = LocalTV.DictVolumeInfo;
            _bCompareMode = bCompareMode;
            _bSecondComparePane = bSecondComparePane;
            _thread = Util.ThreadMake(() => Go(treeNode, strFile, nInitiator));
            return true;
        }

        static void Go(LocalTreeNode treeNode, string strFile, int nInitiator)
        {
            GetFileList(treeNode, strFile, nInitiator);
            GetFolderDetail(treeNode, nInitiator);
            GetVolumeDetail(treeNode, nInitiator);
            _thread = null;
        }

        static void GetFileList(LocalTreeNode treeNode, string strFile, int nInitiator)
        {
            string strListingFile = null;
            IEnumerable<string> lsFiles = null;

            Util.Closure(() =>
            {
                var nodeDatum = treeNode.NodeDatum;
                var rootNode = treeNode.Root;

                if ((null == nodeDatum) ||
                    (0 == nodeDatum.LineNo) ||
                    (false == rootNode.NodeDatum is RootNodeDatum))
                {
                    return;     // from lambda
                }

                var rootNodeDatum = (RootNodeDatum)rootNode.NodeDatum;

                strListingFile = rootNodeDatum.ListingFile;

                var nPrevDir = (int)nodeDatum.PrevLineNo;
                var nLineNo = (int)nodeDatum.LineNo;

                if (0 == nPrevDir)
                    return;     // from lambda

                if (1 >= (nLineNo - nPrevDir))  // dir has no files
                    return;     // from lambda

                lsFiles =
                    File
                    .ReadLines(strListingFile)
                    .Skip(nPrevDir)
                    .Take((nLineNo - nPrevDir - 1))
                    .ToArray();
            });

            FileListUpdatedOnNext(Tuple.Create(lsFiles, strListingFile, treeNode, strFile), nInitiator);
        }

        static void GetFolderDetail(LocalTreeNode treeNode, int nInitiator)
        {
            var nodeDatum = treeNode.NodeDatum;
            var lasItems = new List<IEnumerable<string>>();

            if ((null == nodeDatum) ||
                (0 == nodeDatum.LineNo))
            {
                FolderDetailUpdatedOnNext(Tuple.Create(lasItems.AsEnumerable(), (LocalTreeNode)null), nInitiator);
                return;
            }

            const string kStrFmt_thous = "###,###,###,##0";

            lasItems.Add(new[] { "# Files Here", nodeDatum.FilesHere.ToString(kStrFmt_thous) });
            lasItems.Add(new[] { "with Size of", FormatSize(nodeDatum.Length, bBytes: true) });
            lasItems.Add(new[] { "Total # Files", nodeDatum.FilesInSubdirs.ToString(kStrFmt_thous) });
            lasItems.Add(new[] { "# Folders Here", ((null != treeNode.Nodes) ? treeNode.Nodes.Length : 0).ToString(kStrFmt_thous) });

            if (0 < nodeDatum.SubDirs)
            {
                var strItem = nodeDatum.SubDirs.ToString(kStrFmt_thous);

                if (0 < nodeDatum.DirsWithFiles)
                {
                    var nDirsWithFiles = nodeDatum.DirsWithFiles;

                    if (0 < nodeDatum.FilesHere)
                        --nDirsWithFiles;

                    if (0 < nDirsWithFiles)
                        strItem += " (" + nDirsWithFiles.ToString(kStrFmt_thous) + " with files)";
                }

                lasItems.Add(new[] { "# Subfolders", strItem });
            }

            lasItems.Add(new[] { "Total Size", FormatSize(nodeDatum.TotalLength, bBytes: true) });
            FolderDetailUpdatedOnNext(Tuple.Create(lasItems.AsEnumerable(), treeNode), nInitiator);
        }

        static void GetVolumeDetail(LocalTreeNode treeNode, int nInitiator)
        {
            var rootNode = treeNode.Root;
            var strDriveInfo = _dictVolumeInfo.TryGetValue(((RootNodeDatum)rootNode.NodeDatum).ListingFile);

            if ((null == _dictVolumeInfo) ||
                (null == strDriveInfo))
            {
                VolumeDetailUpdatedOnNext(Tuple.Create((IEnumerable<IEnumerable<string>>)null, rootNode.Text), nInitiator);
                return;
            }

            var arrDriveInfo =
                strDriveInfo
                .Split(new[] { "\r\n", "\n" },
                StringSplitOptions.None);

            MBoxStatic.Assert(1301.2314m,
                new[] { 7, 8, 10, kanDIviewOrder.Length }
                .Contains(arrDriveInfo.Length));

            var asItems = new string[arrDriveInfo.Length][];

            for (var i = 0; i < arrDriveInfo.Length; ++i)
            {
                var a = arrDriveInfo[i].Split('\t');

                if (0 == a[1].Trim().Length)
                    continue;

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

                var ixA =
                    (arrDriveInfo.Length == kanDIviewOrder.Length)
                    ? kanDIviewOrder[ix]
                    : ix;

                lasItems.Add(asItems[ix]);
            }

            VolumeDetailUpdatedOnNext(Tuple.Create(lasItems.Where(i => null != i), rootNode.Text), nInitiator);
        }

        static IDictionary<string, string>
            _dictVolumeInfo = null;
        static bool
            _bCompareMode = false;
        static bool
            _bSecondComparePane = false;
        static Thread
            _thread = null;
    }
}
