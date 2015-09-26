using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace DoubleFile
{
    class LVitem_ClonesVM : ListViewItemVM_Base, IListUpdater
    {
        public ICommand Icmd_NextClonePath { get; }

        public Visibility VisibilityOnClones => 2 <= (TreeNodes?.Count ?? 0) ? Visibility.Visible : Visibility.Collapsed;

        // marker node if treenode not present
        public string Text => WithLocalTreeNode(t => t)?.Text ?? SubItems[0];
        public Brush Foreground => WithLocalTreeNode(t => t.Foreground) ?? Brushes.White;
        public Brush Background => WithLocalTreeNode(t => t.Background) ?? Brushes.DarkSlateGray;
        public FontWeight FontWeight => (0 < TreeNodes.Count) ? FontWeights.Normal : FontWeights.Bold;

        public string       // includes the subject node: only note three clones or more
            Clones => (3 <= TreeNodes.Count) ? (TreeNodes.Count - 1).ToString("###,###") : null;

        public string
            ClonePaths => WithLocalTreeNode(t =>
        {
            if (null == NicknameUpdater)
                return null;    // marker item

            NicknameUpdater.LastGet(this);
            return t.FullPathGet(NicknameUpdater.Value);
        });

        void IListUpdater.RaiseListUpdate() => RaisePropertyChanged("ClonePaths");

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 1;

        internal IList<LocalTreeNode>
            TreeNodes = new LocalTreeNode[0];

        internal T
            WithLocalTreeNode<T>(Func<LocalTreeNode, T> doSomethingWith) =>
            (0 < TreeNodes.Count) ? doSomethingWith(TreeNodes[_clonePathIndex % TreeNodes.Count]) : default(T);

        internal LVitem_ClonesVM(IList<string> asString)
            : base(null, asString)
        {
        }

        internal LVitem_ClonesVM(IList<LocalTreeNode> treeNodes, ListUpdater<bool> nicknameUpdater)
        {
            TreeNodes = treeNodes;
            NicknameUpdater = nicknameUpdater;

            Icmd_NextClonePath =
                new RelayCommand(() =>
            {
                ++_clonePathIndex;
                RaisePropertyChanged("ClonePaths");
                RaisePropertyChanged("Folder");
            });
        }

        int _clonePathIndex = 0;

        internal readonly ListUpdater<bool>
            NicknameUpdater;
    }
}
