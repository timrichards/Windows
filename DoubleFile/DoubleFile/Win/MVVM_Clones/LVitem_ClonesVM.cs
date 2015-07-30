using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DoubleFile
{
    class LVitem_ClonesVM : ListViewItemVM_Base
    {
        public ICommand Icmd_NextClonePath { get; }

        // marker node if treenode not present
        public string Folder => WithLocalTreeNode(t => t)?.Text ?? SubItems[0];
        public Brush Foreground => WithLocalTreeNode(t => t.Foreground) ?? Brushes.White;
        public Brush Background => WithLocalTreeNode(t => t.Background) ?? Brushes.DarkSlateGray;
        public FontWeight FontWeight => (0 < TreeNodes.Count) ? FontWeights.Normal : FontWeights.Bold;

        public string       // includes the subject node: only note three clones or more
            Clones => (3 <= TreeNodes.Count) ? (TreeNodes.Count - 1).ToString("###,###") : null;

        public string ClonePaths => WithLocalTreeNode(t => t.FullPath);

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 1;

        internal TabledString<Tabled_Folders>
            Name;
        //internal LocalLVitem
        //    IgnoreLVItem;
        internal IList<LocalTreeNode>
            TreeNodes = new LocalTreeNode[0];

        internal int
            Index = 0;

        internal T
            WithLocalTreeNode<T>(Func<LocalTreeNode, T> doSomethingWith) =>
            (0 < TreeNodes.Count) ? doSomethingWith(TreeNodes[_clonePathIndex % TreeNodes.Count]) : default(T);

        internal LVitem_ClonesVM(IList<string> asString = null)
            : base(null, asString)
        {
            Icmd_NextClonePath =
                new RelayCommand(() =>
            {
                ++_clonePathIndex;
                RaisePropertyChanged("ClonePaths");
                RaisePropertyChanged("Folder");
            });
        }

        int _clonePathIndex = 0;
    }
}
