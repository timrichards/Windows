﻿using System.Collections.Generic;
using System.Threading;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class TreeSelect : FileParse
    {
        internal class FileListUpdated
        {
            internal readonly IEnumerable<string> ieFiles;
            internal readonly LocalTreeNode treeNode;
            internal readonly string strFilename;

            internal FileListUpdated(IEnumerable<string> ieFiles_, LocalTreeNode treeNode_, string strFilename_)
            {
                ieFiles = ieFiles_;
                treeNode = treeNode_;
                strFilename = strFilename_;
            }

            static internal readonly IObservable<Tuple<FileListUpdated, decimal>>
                Observable = new LocalSubject<FileListUpdated>();
        }
        static void
            FileListUpdatedOnNext(FileListUpdated value, decimal nInitiator) =>
            ((LocalSubject<FileListUpdated>)FileListUpdated.Observable).LocalOnNext(value, 99846, nInitiator);

        internal class FolderDetailUpdated
        {
            internal readonly IEnumerable<IEnumerable<string>> ieDetail;
            internal readonly LocalTreeNode treeNode;

            internal FolderDetailUpdated(IEnumerable<IEnumerable<string>> ieDetail_, LocalTreeNode treeNode_)
            {
                ieDetail = ieDetail_;
                treeNode = treeNode_;
            }

            static internal readonly IObservable<Tuple<FolderDetailUpdated, decimal>>
                Observable = new LocalSubject<FolderDetailUpdated>();
        }
        static void
            FolderDetailUpdatedOnNext(FolderDetailUpdated value, decimal nInitiator) =>
            ((LocalSubject<FolderDetailUpdated>)FolderDetailUpdated.Observable).LocalOnNext(value, 99845, nInitiator);

        internal class VolumeDetailUpdated
        {
            internal readonly IEnumerable<IEnumerable<string>> ieDetail;
            internal readonly string strVolume;

            internal VolumeDetailUpdated(IEnumerable<IEnumerable<string>> ieDetail_, string strVolume_)
            {
                ieDetail = ieDetail_;
                strVolume = strVolume_;
            }

            static internal readonly IObservable<Tuple<VolumeDetailUpdated, decimal>>
                Observable = new LocalSubject<VolumeDetailUpdated>();
        }
        static void
            VolumeDetailUpdatedOnNext(VolumeDetailUpdated value, decimal nInitiator) =>
            ((LocalSubject<VolumeDetailUpdated>)VolumeDetailUpdated.Observable).LocalOnNext(value, 99844, nInitiator);

        static internal bool
            DoThreadFactory(LocalTreeNode treeNode, decimal nInitiator, string strFile = null,
            bool bCompareMode = false, bool bSecondComparePane = false)
        {
            if (_thread?.IsAlive ?? false)
                return false;

            if (treeNode is LocalTreemapNode)     // does not support file fake nodes
                return false;

            _dictVolumeInfo = LocalTV.DictVolumeInfo;
            _bCompareMode = bCompareMode;
            _bSecondComparePane = bSecondComparePane;
            _thread = Util.ThreadMake(() => Go(treeNode, strFile, nInitiator));
            return true;
        }

        static void
            Go(LocalTreeNode treeNode, string strFile, decimal nInitiator)
        {
            FileListUpdatedOnNext(new FileListUpdated(treeNode.GetFileList(), treeNode, strFile), nInitiator);
            GetFolderDetail(treeNode, nInitiator);
            GetVolumeDetail(treeNode, nInitiator);
            _thread = null;
        }

        static void
            GetFolderDetail(LocalTreeNode treeNode, decimal nInitiator)
        {
            var nodeDatum = treeNode.NodeDatum;
            var lieDetail = new List<IEnumerable<string>>();
            const string kStrFmt_thous = "###,###,###,##0";

            lieDetail.Add(new[] { "# Files Here", nodeDatum.FileCountHere.ToString(kStrFmt_thous) });
            lieDetail.Add(new[] { "with Size of", nodeDatum.LengthHere.FormatSize(bytes: true) });
            lieDetail.Add(new[] { "Total # Files", nodeDatum.FileCountTotal.ToString(kStrFmt_thous) });
            lieDetail.Add(new[] { "# Folders Here", (treeNode.Nodes?.Count ?? 0).ToString(kStrFmt_thous) });

            if (0 < nodeDatum.SubDirs)
            {
                var strItem = nodeDatum.SubDirs.ToString(kStrFmt_thous);

                if (0 < nodeDatum.DirsWithFiles)
                {
                    var nDirsWithFiles = nodeDatum.DirsWithFiles;

                    if (0 < nodeDatum.FileCountHere)
                        --nDirsWithFiles;

                    if (0 < nDirsWithFiles)
                        strItem += " (" + nDirsWithFiles.ToString(kStrFmt_thous) + " with files)";
                }

                lieDetail.Add(new[] { "# Subfolders", strItem });
            }

            lieDetail.Add(new[] { "Total Size", nodeDatum.LengthTotal.FormatSize(bytes: true) });

            Action<string, DateTime> addDateTime = (string label, DateTime dt) =>
            {
                if (DateTime.MinValue != dt)
                    lieDetail.Add(new[] { label, dt.ToLongDateString() + " " + dt.ToLongTimeString() });
            };

            addDateTime("Created", nodeDatum.FolderDetails.Created);
            addDateTime("Modified", nodeDatum.FolderDetails.Modified);

            if (0 != nodeDatum.FolderDetails.Attributes)
                lieDetail.Add(new[] { "Attributes", "" + Util.DecodeAttributes(nodeDatum.FolderDetails.Attributes) });

            FolderDetailUpdatedOnNext(new FolderDetailUpdated(lieDetail, treeNode), nInitiator);
        }

        static void
            GetVolumeDetail(LocalTreeNode treeNode, decimal nInitiator)
        {
            var rootNode = treeNode.Root;
            var strDriveInfo = _dictVolumeInfo.TryGetValue(rootNode.RootNodeDatum.LVitemProjectVM.ListingFile);

            if ((null == _dictVolumeInfo) ||
                (null == strDriveInfo))
            {
                VolumeDetailUpdatedOnNext(new VolumeDetailUpdated(null, rootNode.PathShort), nInitiator);
                return;
            }

            var arrDriveInfo =
                strDriveInfo
                .Split(new[] { "\r\n", "\n" },
                StringSplitOptions.None);

            Util.Assert(1301.2314m,
                new[] { 7, 8, 10, kanDIviewOrder.Count }
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
                        ? a[1].FormatSize(bytes: true) 
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
                    (arrDriveInfo.Length == kanDIviewOrder.Count)
                    ? kanDIviewOrder[ix]
                    : ix;

                lasItems.Add(asItems[ix]);
            }

            VolumeDetailUpdatedOnNext(new VolumeDetailUpdated(lasItems.Where(i => null != i), rootNode.PathShort), nInitiator);
        }

        static IReadOnlyDictionary<string, string>
            _dictVolumeInfo = null;
        static bool
            _bCompareMode = false;
        static bool
            _bSecondComparePane = false;
        static Thread
            _thread = null;
    }
}
