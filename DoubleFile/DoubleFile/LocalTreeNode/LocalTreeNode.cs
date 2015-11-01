﻿using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    interface ILocalTreeNode_SetLevel
    {
        int Set(IEnumerable<LocalTreeNode> rootNodes);
    }

    [DebuggerDisplay("{NodeDatum.Hash_AllFiles} {PathShort} {Nodes?.Count}")]
    class LocalTreeNode : ILocalTreeNode_SetLevel
    {
        internal NodeDatum
            NodeDatum;
        internal virtual IReadOnlyList<LocalTreeNode>
            Nodes { get; set; }
        internal LocalTreeNode
            Parent;
        internal List<LocalTreeNode>
            Clones;
        internal LVitem_ClonesVM
            LVitem;
        internal LocalTreemapFileListNode
            TreemapFiles;
        internal Rect
            TreemapRect;

        internal LocalTreeNode
            FirstNode => Nodes?.First();
        internal RootNodeDatum
            RootNodeDatum => (RootNodeDatum)Root.NodeDatum;
        internal bool
            IsSolitary => null == Clones;
        internal bool
            IsAllOnOneVolume => UtilColorcode.AllOnOneVolume == Clones?[0].ColorcodeFG;

        internal int ColorcodeFG { get { return UtilColorcode.GetFG_ARGB(Color); } set { int c = Color; Color = UtilColorcode.SetFG_ARGB(ref c, value); } }
        internal int ColorcodeBG { get { return UtilColorcode.GetBG_ARGB(Color); } set { int c = Color; Color = UtilColorcode.SetBG_ARGB(ref c, value); } }

        internal Brush
            Foreground =>
            (UtilColorcode.Transparent == ColorcodeFG)
            ? Brushes.White
            : UtilColorcode.ARGBtoBrush(ColorcodeFG);

        internal Brush
            Background => UtilColorcode.ARGBtoBrush(ColorcodeBG);

        internal int Level
        {
            get { return (int)(_datum_color_level & _knDatum8bitMask) >> UtilColorcode.CLUT_Shift; }
            set { _datum_color_level = (short)((_datum_color_level & (-1 - _knDatum8bitMask)) + (value << UtilColorcode.CLUT_Shift)); }
        }

        internal LocalTreeNode
            Root
        {
            get
            {
                var parent = this;

                for (; null != parent.Parent; parent = parent.Parent)
                    ;

                return parent;
            }
        }

        internal LocalTreeNode()
        {
            Color = UtilColorcode.Set_ARGB(UtilColorcode.Transparent, UtilColorcode.Transparent);
            Level = -1;
        }

        internal LocalTreeNode(string strContent)
            : this()
        {
            PathShort = strContent;
        }

        internal LocalTreeNode(TabledString<TabledStringType_Folders> strContent)
            : this()
        {
            _strPathShort = strContent;
        }

        internal LocalTreeNode(string strContent, IReadOnlyList<LocalTreeNode> lsNodes)
            : this(strContent)
        {
            Nodes = lsNodes;
        }

        internal virtual string
            PathShort
        {
            get
            {
                var rootNodeDatum = NodeDatum.As<RootNodeDatum>();

                return
                    (null != rootNodeDatum)
                    ? "" + rootNodeDatum.LVitemProjectVM.RootText   // if this is a root treenode return nickname text
                    : "" + _strPathShort;
            }
            set { _strPathShort = (TabledString<TabledStringType_Folders>)value; }
        }

        internal string
            PathFull => PathFullGet(true);
        internal string
            PathFullGet(bool UseNickname)
        {
            var sbPath = new StringBuilder();

            for (var treeNode = this; ; treeNode = treeNode.Parent)
            {
                if (null == treeNode.Parent)
                {
                    var strRet = "" + sbPath.Insert(0, UseNickname ? treeNode.PathShort : "" + treeNode._strPathShort).Replace(@"\\", @"\");

                    if (2 == strRet.Length)
                        strRet += '\\';

                    return strRet;
                }

                sbPath
                    .Insert(0, treeNode._strPathShort)
                    .Insert(0, '\\');
            }
        }

        internal bool
            IsChildOf(LocalTreeNode treeNode)
        {
            if (Level <= treeNode.Level)
                return false;

            var parentNode = Parent;

            while (null != parentNode)
            {
                if (parentNode == treeNode)
                    return true;

                parentNode = parentNode.Parent;
            }

            return false;
        }

        int
            ILocalTreeNode_SetLevel.Set(IEnumerable<LocalTreeNode> rootNodes) => SetLevel(rootNodes, null, 0);
        static int SetLevel(IEnumerable<LocalTreeNode> nodes, LocalTreeNode nodeParent, int nLevel)
        {
            var nCount = 0;

            foreach (var treeNode in nodes)
            {
                treeNode.Parent = nodeParent;
                treeNode.Level = nLevel;

                if (null != treeNode.Nodes)
                    nCount += SetLevel(treeNode.Nodes, treeNode, nLevel + 1);

                ++nCount;
            }

            return nCount;
        }

        internal LocalTreeNode
            GoToFile(string strFile)
        {
            TreeSelect.DoThreadFactory(this, 0 /* UI Initiator */, strFile);
            return this;
        }

        internal IReadOnlyList<Tuple<LocalTreeNode, IReadOnlyList<string>>>
            GetFileLinesA(IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>> ieHashesGrouped)
        {
            ReadLinesIterator
                iterator = 
                RootNodeDatum.LVitemProjectVM.ListingFile
                .ReadLines(99596).As<ReadLinesIterator>();

            var currentPos = 0;
            var nHashColumn = Statics.DupeFileDictionary.HashColumn;

            var x =
                ieHashesGrouped
                .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
                .Select(tuple => Tuple.Create(tuple.Item1,
                tuple.Item1
                .GetFileList(ref currentPos, iterator).ToList()
                .Select(strLine => strLine.Split('\t'))
                .Where(asLine => nHashColumn < asLine.Length)
                .DistinctBy(asLine => asLine[nHashColumn])
                .Select(asLine => new { a = HashTuple.FileIndexedIDfromString(asLine[nHashColumn], asLine[FileParse.knColLength]), b = asLine })
                .Where(sel => tuple.Item2.Contains(sel.a))
                .Select(sel => (IReadOnlyList<string>)sel.b.Skip(3).ToArray())))
                .ToList();
        }

        internal IReadOnlyList<Tuple<LocalTreeNode, IReadOnlyList<string>>>
            GetFileLines(IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>>> ieHashesGrouped)
        {
            ReadLinesIterator
                iterator = 
                RootNodeDatum.LVitemProjectVM.ListingFile
                .ReadLines(99596).As<ReadLinesIterator>();

            var currentPos = 0;
            var nHashColumn = Statics.DupeFileDictionary.HashColumn;

            return
                (IReadOnlyList<Tuple<LocalTreeNode, IReadOnlyList<string>>>)
                ieHashesGrouped
                .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
                .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2,
                tuple.Item1
                .GetFileList(ref currentPos, iterator).ToList()
                .Select(strLine => strLine.Split('\t'))
                .Where(asLine => nHashColumn < asLine.Length)
                .DistinctBy(asLine => asLine[nHashColumn])
                .Select(asLine => new { a = HashTuple.FileIndexedIDfromString(asLine[nHashColumn], asLine[FileParse.knColLength]), b = asLine })
                .Where(sel => tuple.Item2.Contains(sel.a))
                .Select(sel => Tuple.Create(tuple.Item1, (IReadOnlyList<string>)sel.b.Skip(3).ToArray()))))
                .ToList();

            //IEnumerable<string>
            //    ieFileLinesGrouped = new string[0];
            //IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>, IEnumerable<string>>>
            //    ieFileLinesGrouped = new Tuple<LocalTreeNode, IReadOnlyList<int>, IEnumerable<string>>[] { };
            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>, IEnumerable<string>>>
                ieFileLinesGrouped = new Tuple<LocalTreeNode, IReadOnlyList<int>, IEnumerable<string>>[] { };
            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>, string>>
                ieFileLinesGroupedA = new Tuple<LocalTreeNode, IReadOnlyList<int>, string>[] { };

            ReadLinesIterator
                iterator = 
                RootNodeDatum.LVitemProjectVM.ListingFile
                .ReadLines(99596).As<ReadLinesIterator>();

            int currentPos = 0;

            //foreach (var tuple in ieHashesGrouped.OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo))
            //    ieFileLinesGrouped = ieFileLinesGrouped.Concat(tuple.Item1.GetFileList(ref currentPos, iterator));
            //foreach (var tuple in ieHashesGrouped.OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo))
            //    ieFileLinesGrouped = ieFileLinesGrouped.Concat(new[] { Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item1.GetFileList(ref currentPos, iterator)) });


            //var x =
            //    ieHashesGrouped
            //    .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
            //    .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2,
            //    tuple.Item1
            //    .GetFileList(ref currentPos, iterator)))
            //    .SelectMany(y => y.Item3)
            //    .ToList();

            //ieFileLinesGroupedA =
            //    ieHashesGrouped
            //    .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
            //    .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2,
            //    tuple.Item1
            //    .GetFileList(ref currentPos, iterator)))
            //    .SelectMany(tuple => tuple.Item3
            //    .Select(strLine => Tuple.Create(tuple.Item1, tuple.Item2, strLine)));

            //ieFileLinesGroupedA =
            //    ieHashesGrouped
            //    .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
            //    .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2,
            //    tuple.Item1
            //    .GetFileList(ref currentPos, iterator)))
            //    .SelectMany(tuple => tuple.Item3, (tuple, strLine) => Tuple.Create(tuple.Item1, tuple.Item2, strLine));

            //ieFileLinesGrouped =
            //    ieHashesGrouped
            //    .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
            //    .Select(tuple => Tuple.Create(tuple.Item1, tuple.Item2,
            //    tuple.Item1
            //    .GetFileList(ref currentPos, iterator)));

            //ieFileLinesGroupedA =
            //    ieHashesGrouped
            //    .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
            //    .Select(tuple =>
            //    tuple.Item1
            //    .GetFileList(ref currentPos, iterator)
            //    .Select(strLine => Tuple.Create(tuple.Item1, tuple.Item2, strLine)))
            //    .SelectMany(g => g, (g, a) => a);

            //ieFileLinesGroupedA =
            //    ieHashesGrouped
            //    .OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo)
            //    .Select(tuple =>
            //    tuple.Item1
            //    .GetFileList(ref currentPos, iterator)
            //    .Select(strLine => Tuple.Create(tuple.Item1, tuple.Item2, strLine)))
            //    .SelectMany(g => g);

            //foreach (var tuple in ieHashesGrouped.OrderBy(tuple => tuple.Item1.NodeDatum.PrevLineNo))
            //{
            //    ieFileLinesGroupedA = ieFileLinesGroupedA.Concat(
            //        tuple.Item1
            //        .GetFileList(ref currentPos, iterator)
            //        .Select(strLine => Tuple.Create(tuple.Item1, tuple.Item2, strLine)));

            //    ++nCount;
            //}

            //var ls = ieFileLinesGrouped.ToList();

            //foreach (var tuple in ieFileLinesGrouped)
            //{
            //    ieFileLinesGroupedA = ieFileLinesGroupedA.Concat(
            //        tuple.Item3
            //        .Select(strLine => Tuple.Create(tuple.Item1, tuple.Item2, strLine)));
            //}

            var ls = ieFileLinesGroupedA.ToList();

            //ieFileLinesGrouped
                //.SelectMany(tuple => tuple.Item3, (tuple, strLine) => Tuple.Create(tuple.Item1, tuple.Item2, strLine))
                //.ToList();    // ToList() enumerates: reads through the file exactly once then closes it

            //IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>, IEnumerable<string[]>>>
            //    ieFileLinesGroupedA = new Tuple<LocalTreeNode, IReadOnlyList<int>, IEnumerable<string[]>>[] { };

            IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<string>>> ieFiles = new Tuple<LocalTreeNode, IReadOnlyList<string>>[] { };

            // from:    IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<int>, IEnumerable<string>>>
            // to:      IEnumerable<Tuple<LocalTreeNode, IReadOnlyList<string>>>

            //foreach (var tuple in lsFiles_in.SelectMany(tuple => tuple.Item2, (tuple, x) => tuple).DistinctBy(tuple => tuple.Item2))
            //lsFileLinesGrouped.SelectMany(tuple => tuple.Item3, (tuple, x) => tuple)
            //    .Select(strLine => strLine.Split('\t'))
            //    .Where(asLine => nHashColumn < asLine.Length)
            //    .DistinctBy(asLine => asLine[nHashColumn])
            //    .Select(asLine => new { a = HashTuple.FileIndexedIDfromString(asLine[nHashColumn], asLine[FileParse.knColLength]), b = asLine })
            //    .Where(sel => tuple.Item2.Contains(sel.a))
            //    .Select(sel => Tuple.Create(tuple.Item1, (IReadOnlyList<string>)sel.b.Skip(3).ToArray())));

            //foreach (var tuple in lsFileLinesGrouped)
            {
                //ieFiles =
                //    ieFiles.Concat(
                //    tuple.Item3
                //    .Select(strLine => strLine.Split('\t'))
                //    .Where(asLine => nHashColumn < asLine.Length)
                //    .DistinctBy(asLine => asLine[nHashColumn])
                //    .Select(asLine => new { a = HashTuple.FileIndexedIDfromString(asLine[nHashColumn], asLine[FileParse.knColLength]), b = asLine })
                //    .Where(sel => tuple.Item2.Contains(sel.a))
                //    .Select(sel => Tuple.Create(tuple.Item1, (IReadOnlyList<string>)sel.b.Skip(3).ToArray())));

                //ieFiles =
                //    ieFiles.Concat(
                //    tuple.Item3
                //    .Select(strLine => strLine.Split('\t'))
                //    .Where(asLine => nHashColumn < asLine.Length)
                //    .DistinctBy(asLine => asLine[nHashColumn])
                //    .Select(asLine => new { a = HashTuple.FileIndexedIDfromString(asLine[nHashColumn], asLine[FileParse.knColLength]), b = asLine })
                //    .Where(sel => tuple.Item2.Contains(sel.a))
                //    .Select(sel => Tuple.Create(tuple.Item1, (IReadOnlyList<string>)sel.b.Skip(3).ToArray())));

                ////          tuple.Item3
                ////           .AsParallel()
                ////.Select(strLine => strLine.Split('\t'))
                ////.Where(asLine => nHashColumn < asLine.Length)
                ////.DistinctBy(asLine => asLine[nHashColumn])
                ////.Select(asLine => new { a = 0/*HashTuple.FileIndexedIDfromString(asLine[nHashColumn], asLine[FileParse.knColLength])*/, b = 0/*asLine*/ })
                ////  .Where(sel => tuple.Item2.Contains(sel.a))
                ////     .Select(sel => 
                //new[] { Tuple.Create(tuple.Item1, (IReadOnlyList<string>)new string[0]) })
                //;//); //sel.b.Skip(3).ToArray())));
            }                                       // makes this an LV line: knColLengthLV----^

            var lsRet = ieFiles.OrderBy(tuple => tuple.Item2[0]).ToList();

            return lsRet;
        }

        internal IEnumerable<string>
            GetFileList() { int x = 0; return GetFileList(ref x); }
        IEnumerable<string>
            GetFileList(ref int currentPos, ReadLinesIterator iterator = null)
        {
            var nPrevDir = (int)NodeDatum.PrevLineNo;

            if (0 == nPrevDir)
                return new string[0];

            if (0 == NodeDatum.FileCountHere)
                return new string[0];

            if (null != iterator)
            {
                var ieRet =
                    iterator
                    .Skip(nPrevDir - currentPos)
                    .Take(NodeDatum.FileCountHere);

                currentPos = nPrevDir + NodeDatum.FileCountHere;
                return ieRet;
            }

            return
                RootNodeDatum.LVitemProjectVM.ListingFile
                .ReadLines(99643)
                .Skip(nPrevDir)
                .Take(NodeDatum.FileCountHere)
                .ToArray();
        }

        int Color
        {
            get { return _datum_color_level & UtilColorcode.CLUT_Mask; }
            set { _datum_color_level = (short)((_datum_color_level & (-1 - UtilColorcode.CLUT_Mask)) + value); }
        }

        short
            _datum_color_level = 0;

        TabledString<TabledStringType_Folders>
            _strPathShort;
        static readonly uint
            _knDatum8bitMask = (1 << 16) - 1 - UtilColorcode.CLUT_Mask;
    }
}
