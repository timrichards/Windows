using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

namespace SearchDirLists
{
    public static class TVI_DependencyProperty
    {
        public static readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached
        ("Event", typeof(bool), typeof(TVI_DependencyProperty), new UIPropertyMetadata(false, OnDPchanged));

        public static bool GetEvent(FrameworkElement element) { return (bool)element.GetValue(EventProperty); }
        public static void SetEvent(FrameworkElement element, bool value) { element.SetValue(EventProperty, value); }

        // This is where you modify (a) the type; and (b) the event handled.
        static void OnDPchanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem tvife = depObj as TreeViewItem;
            bool bAddEvt = (bool)e.NewValue;

            if (bAddEvt)
            {
                tvife.Loaded += OnLoaded;
                tvife.Selected += OnSelect;
            }
            else
            {
                tvife.Loaded -= OnLoaded;
                tvife.Selected -= OnSelect;
            }
        }

        static void OnLoaded(object sender, EventArgs e)
        {
            if (TVI_DependencyProperty.HeaderHeight > 0)
            {
                return;
            }

            TreeViewItem tvife = sender as TreeViewItem;
            FrameworkElement header = (FrameworkElement)tvife.Template.FindName("PART_Header", tvife);

            if (header != null)
            {
                TVI_DependencyProperty.HeaderHeight = header.ActualHeight;
            }
        }

        static void OnSelect(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource))
            {
                WaitingToSelect = null;

                TreeViewItem tvife = sender as TreeViewItem;
                TreeViewItemVM tvivm = (TreeViewItemVM)tvife.DataContext;

                if (tvivm.m_bBringIntoViewWhenSel)
                {
                    tvife.BringIntoView();
                }
            }
        }

        internal static void OnTimer(object o, EventArgs e)
        {
            if (WaitingToSelect == null)
            {
                return;
            }

            if (stackParents.Count > 0)
            {
                scrollViewer.ScrollToVerticalOffset(stackParents.Pop().EphemeralExpandedPos);
            }
            else
            {
                scrollViewer.ScrollToVerticalOffset(WaitingToSelect.EphemeralExpandedPos);
                WaitingToSelect = null;
            }
        }

        internal static TreeView TVFE = null;
        internal static double HeaderHeight = -1;
        internal static Stack<TreeViewItemVM> stackParents = null;

        internal static TreeViewItemVM WaitingToSelect = null;

        internal static ScrollViewer scrollViewer
        {
            get { return m_scrollViewer_ ?? (m_scrollViewer_ = TVFE.Template.FindName("_tv_scrollviewer_", TVFE) as ScrollViewer); }
        }
        static ScrollViewer m_scrollViewer_ = null;
    }

    public class TreeViewItemVM : ObservableObject
    {
        public ObservableCollection<TreeViewItemVM> Items { get { return m_Items; } }
        public String Text { get { return datum.Text; } }
#if (WPF)
        public Brush Foreground { get { return SDLWPF._ForeClrToBrush(datum.ForeColor); } }
        public Brush Background { get { return SDLWPF._BackClrToBrush(datum.BackColor); } }
#endif
        public bool IsExpanded
        {
            get { return m_bExpanded; }
            set
            {
                if (value == m_bExpanded)
                {
                    return;
                }

                m_bExpanded = value;

                if (m_bExpanded)
                {
                    TVVM.m_listExpanded.Add(this);
                }
                else
                {
                    TVVM.m_listExpanded.Remove(this);
                }
            }
        }

        public bool IsSelected
        {
            get { return m_bSelected; }
            set
            {
                if (value == m_bSelected)
                {
                    return;
                }

                Utilities.WriteLine("IsSelected " + Text);
                m_bSelected = value;

                if (datum.LVIVM != null)
                {
                    if (m_bSelected)
                    {
                        datum.LVIVM.LVVM.LVFE.ScrollIntoView(datum.LVIVM.LVVM.LVFE.Items[datum.LVIVM.Index]);
                    }

                    datum.LVIVM.SelectProgrammatic(m_bSelected);
                }

                if (m_bSelected)
                {
                    TVVM.SelectedItem = this;
                }
                else if (TVVM.SelectedItem == this)
                {
                    TVVM.SelectedItem = null;
                }
            }
        }

        internal void SelectProgrammatic(bool bSelect)
        {
            if (m_bSelected == bSelect)
            {
                return;
            }

            if (bSelect)
            {
                TVI_DependencyProperty.WaitingToSelect = this;
                m_bBringIntoViewWhenSel = true;
            }

            m_bSelected = bSelect;
            RaisePropertyChanged("IsSelected");

            if (bSelect == false)
            {
                EphemeralExpandedPos = -1;
                return;
            }

            TVVM.SelectedItem = this;

            Stack<TreeViewItemVM> stackParents = new Stack<TreeViewItemVM>(8);
            TVI_DependencyProperty.stackParents = new Stack<TreeViewItemVM>();
            UList<TreeViewItemVM> listParents = new UList<TreeViewItemVM>();
            TreeViewItemVM parentItem = m_Parent;

            while (parentItem != null)
            {
                stackParents.Push(parentItem);
                TVI_DependencyProperty.stackParents.Push(parentItem);
                listParents.Add(parentItem);
                parentItem = parentItem.m_Parent;
            }

            foreach (TreeViewItemVM tvivm in TVVM.m_listExpanded)
            {
                if ((stackParents.Contains(tvivm) == false) && tvivm.m_bExpanded)
                {
                    tvivm.EphemeralExpandedPos = -1;
                    tvivm.m_bExpanded = false;
                    tvivm.RaisePropertyChanged("IsExpanded");
                }
            }

            while (stackParents.Count > 0)
            {
                parentItem = stackParents.Pop();
                EphemeralExpandedPos += (parentItem.Index + 1);
                parentItem.EphemeralExpandedPos = EphemeralExpandedPos * HeaderHeight;

                if (parentItem.m_bExpanded == false)
                {
                    parentItem.m_bExpanded = true;
                    parentItem.RaisePropertyChanged("IsExpanded");
                    TVVM.m_listExpanded.Add(parentItem);
                }
            }

            EphemeralExpandedPos += (Index + 1);
            EphemeralExpandedPos *= HeaderHeight;       // when implementing variable-height headers this calc will be wrong
            TVVM.m_listExpanded = listParents;
        }

        internal bool m_bBringIntoViewWhenSel = false;

        internal TreeViewItemVM(TreeViewVM tvvm, SDL_TreeNode datum_in, int nIndex)
            : this(tvvm, datum_in, null, nIndex)
        { }

        TreeViewItemVM(TreeViewVM tvvm, SDL_TreeNode datum_in, TreeViewItemVM parent, int nIndex)
        {
            TVVM = tvvm;
            datum = datum_in;
            m_Parent = parent;
            Index = nIndex;
#if (WPF)
            datum.TVIVM = this;

            int nIndex_A = -1;

            m_Items = new ObservableCollection<TreeViewItemVM>
            (
                (from item in datum.Nodes.Keys select new TreeViewItemVM(tvvm, item, this, ++nIndex_A))
                .ToList<TreeViewItemVM>()
            );
#endif
        }

        readonly ObservableCollection<TreeViewItemVM> m_Items = null;
        internal readonly TreeViewItemVM m_Parent = null;
        internal readonly TreeViewVM TVVM = null;

        double HeaderHeight { get { return TVI_DependencyProperty.HeaderHeight; } }
        internal double EphemeralExpandedPos = -1;

        internal readonly SDL_TreeNode datum = null;

        bool m_bExpanded = false;
        bool m_bSelected = false;
        internal int Index = -1;
    }

    class TreeViewVM
    {
        internal TreeViewVM(TreeView tvfe, Dispatcher dispatcher)
        {
            TVI_DependencyProperty.TVFE = TVFE = tvfe;
            m_scrollTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(20), DispatcherPriority.Normal, new EventHandler(TVI_DependencyProperty.OnTimer), dispatcher);
        }

        internal void SetData(List<SDL_TreeNode> rootNodes)
        {
            int nIndex = -1;

            foreach (SDL_TreeNode treeNode in rootNodes)
            {
                m_Items.Add(new TreeViewItemVM(this, treeNode, ++nIndex));
            }

            TVFE.DataContext = m_Items;
        }

        internal TreeViewItemVM SelectedItem = null;
        internal UList<TreeViewItemVM> m_listExpanded = new UList<TreeViewItemVM>();
        
        readonly List<TreeViewItemVM> m_Items = new List<TreeViewItemVM>();
        internal readonly TreeView TVFE = null;

        internal DateTime mDbg_dtProgrammaticExpand = DateTime.MinValue;
        readonly DispatcherTimer m_scrollTimer = null; 
    }
}
