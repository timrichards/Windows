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
                tvife.Expanded += OnExpand;
                tvife.Loaded += OnLoaded;
                tvife.Unselected += OnDeselect;
                tvife.Selected += OnSelect;
            }
            else
            {
                tvife.Expanded -= OnExpand;
                tvife.Loaded -= OnLoaded;
                tvife.Unselected -= OnDeselect;
                tvife.Selected -= OnSelect;
            }
        }

        static void OnExpand(object sender, EventArgs e)
        {
            if (WaitingToSelect == null)
            {
                return;
            }

            if (stackParents.Count > 0)
            {
                Scroll(stackParents.Pop());
            }
        }

        static void OnLoaded(object sender, EventArgs e)
        {
            if (TVI_DependencyProperty.HeaderHeight > 0)
            {
                return;
            }

            TreeViewItem tvife = sender as TreeViewItem;

            if (tvife == null)
            {
                return;
            }

            FrameworkElement header = (FrameworkElement)tvife.Template.FindName("PART_Header", tvife);

            if (header != null)
            {
                TVI_DependencyProperty.HeaderHeight = header.ActualHeight;
            }
        }

        static void OnDeselect(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource))
            {
                TreeViewItem tvife = sender as TreeViewItem;

                if (tvife == null)
                {
                    return;
                }

                TreeViewItemVM tvivm = tvife.DataContext as TreeViewItemVM;

                if (tvivm == null)      // DisconnectedItem
                {
                    return;
                }

                tvivm.RaisePropertyChanged("Foreground");
                tvivm.RaisePropertyChanged("SelectedForeground");
                tvivm.RaisePropertyChanged("FontWeight");
            }
        }

        static void OnSelect(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource) == false)
            {
            }

            TreeViewItem tvife = sender as TreeViewItem;

            if (tvife == null)
            {
                return;
            }

            TreeViewItemVM tvivm = tvife.DataContext as TreeViewItemVM;

            if (tvivm == null)      // DisconnectedItem
            {
                tvife.BringIntoView();
                return;
            }

            TreeViewItemVM waitingToSelect = WaitingToSelect;

            if (tvivm == waitingToSelect)   // LV programmatic
            {
                tvife.BringIntoView();
                SetWaitingToSelect(null);
                nBringIntoViewAttempts = -1;

                Point ptTVI = tvife.TranslatePoint(new Point(0, 0), scrollViewer);

                if ((ptTVI.Y >= 0) && (ptTVI.Y < scrollViewer.ViewportHeight))
                {
                    Utilities.WriteLine("ViewportHeight A " + tvivm.Text);
                    tvivmSelected = tvivm;
                    nBringIntoViewAttempts = -1;
                    m_tvife = null;
                    return;
                }

                tvivm.m_SelectedForeground = tvivm.Foreground;
                Utilities.WriteLine("Set nBringIntoViewAttempts " + tvivm.Text);
                m_tvife = tvife;
                nBringIntoViewAttempts = 0;
                StartTimer("5 " + tvivm.Text);
            }
            else if (Object.ReferenceEquals(sender, e.OriginalSource) == false)
            {
                return;
            }
            else    // direct treeview selection
            {
                SetWaitingToSelect(null);
                m_tvife = null;
                nBringIntoViewAttempts = -1;
                tvivm.m_SelectedForeground = Brushes.White;
            }

            tvivm.RaisePropertyChanged("Foreground");
            tvivm.RaisePropertyChanged("SelectedForeground");
            tvivm.RaisePropertyChanged("FontWeight");
        }

        internal static void OnTimer(object o, EventArgs e)
        {
            StopTimer(1);

            if (nBringIntoViewAttempts >= 0)
            {
                if (m_tvife == null)
                {
                    Utilities.WriteLine("OnTimer m_tvife == null");
                    return;
                }

                TreeViewItemVM tvivm = m_tvivm;

                if (tvivm == null)      // DisconnectedItem
                {
                    Utilities.WriteLine("OnTimer tvivm == null");
                    return;
                }

                Point ptTVI = m_tvife.TranslatePoint(new Point(0, 0), scrollViewer);

                if ((ptTVI.Y >= 0) && (ptTVI.Y < scrollViewer.ViewportHeight))
                {
                    Utilities.WriteLine("ViewportHeight " + nBringIntoViewAttempts + " " + tvivm.Text);
                    tvivmSelected = tvivm;
                    nBringIntoViewAttempts = -1;
                    m_tvife = null;
                    return;
                }

                if (nBringIntoViewAttempts < 20)
                {
                    nBringIntoViewAttempts += 20;
                }

                switch (nBringIntoViewAttempts)
                {
                    case 20:
                        scrollViewer.ScrollToTop();
                        break;

                    case 21:
                        m_tvife.BringIntoView();
                        break;

                    case 22:
                        scrollViewer.PageUp();
                        break;

                    case 23:
                        m_tvife.BringIntoView();
                        break;

                    case 24:
                        scrollViewer.PageDown();
                        break;

                    case 25:
                        scrollViewer.PageDown();
                        break;

                    case 26:
                        m_tvife.BringIntoView();
                        break;

                    case 27:
                    default:
                        Utilities.WriteLine("default " + tvivm.Text);
                        m_tvife = null;
                        nBringIntoViewAttempts = -1;
                        return;
                }

                ++nBringIntoViewAttempts;
                StartTimer("1 " + tvivm.Text);
                Utilities.WriteLine("nBringIntoViewAttempts " + nBringIntoViewAttempts + " " + tvivm.Text);
                return;
            }

            if (WaitingToSelect == null)
            {
                Utilities.WriteLine("OnTimer WaitingToSelect == null");
                return;
            }

            TreeViewItemVM tvivm_A = null;

            if (stackParents.Count > 0)
            {
                tvivm_A = stackParents.Pop();
            }
            else
            {
                if (new double[] { WaitingToSelect.EphemeralExpandedPos, 0, scrollViewer.ScrollableHeight }.Contains(scrollViewer.VerticalOffset))
                {
                    if (++nAttempts > 0)
                    {
                        SetWaitingToSelect(null);
                        Utilities.WriteLine("OnTimer SetWaitingToSelect(null)");
                        return;
                    }

                    stackParents = stackParents_A;
                }

                tvivm_A = WaitingToSelect;
            }

            Scroll(tvivm_A);
            Utilities.WriteLine("OnTimer end");
        }

        static void Scroll(TreeViewItemVM tvivm)
        {
            StopTimer(3);
            Utilities.WriteLine(tvivm.datum.Level + " " + tvivm.Text + " " + tvivm.EphemeralExpandedPos);
            scrollViewer.ScrollToTop();
            scrollViewer.ScrollToVerticalOffset(tvivm.EphemeralExpandedPos);
            StartTimer("3 " + tvivm.Text);
        }

        internal static TreeView TVFE
        {
            get { return m_TVFE; }
            set
            {
                m_TVFE = value;

                if (m_scrollTimer != null)
                {
                    m_scrollTimer.IsEnabled = false;
                }

                m_scrollTimer = new SDL_Timer(TimeSpan.FromMilliseconds(1000), DispatcherPriority.Normal, new EventHandler(TVI_DependencyProperty.OnTimer), TVFE.Dispatcher);
                StopTimer(2);
            }
        }
        static TreeView m_TVFE = null;

        static internal void StartTimer(string text) { m_scrollTimer.Stop(); m_scrollTimer.Start(); Utilities.WriteLine("StartTimer " + text); }
        static internal void StopTimer(int nWhere) { m_scrollTimer.Stop(); Utilities.WriteLine("StopTimer " + nWhere); }
        internal static TreeViewItemVM WaitingToSelect { get { return m_WaitingToSelect; } }
        internal static void SetWaitingToSelect(TreeViewItemVM tvivm)
        {
            m_WaitingToSelect = tvivm;

            if (m_WaitingToSelect != null)
            {
                stackParents_A = new Stack<TreeViewItemVM>(stackParents.Reverse());
                StartTimer("0 " + tvivm.Text);
            }
            else
            {
                stackParents = stackParents_A = new Stack<TreeViewItemVM>(8);
                nAttempts = -1;
                StopTimer(0);
            }
        }

        static TreeViewItemVM m_WaitingToSelect = null;

        internal static ScrollViewer scrollViewer
        {
            get { return m_scrollViewer_ ?? (m_scrollViewer_ = TVFE.Template.FindName("_tv_scrollviewer_", TVFE) as ScrollViewer); }
        }
        static ScrollViewer m_scrollViewer_ = null;

        internal static TreeViewItemVM tvivmSelected = null;
        internal static double HeaderHeight = -1;
        internal static Stack<TreeViewItemVM> stackParents = null;
        static Stack<TreeViewItemVM> stackParents_A = null;
        static SDL_Timer m_scrollTimer = null;
        static int nAttempts = -1;
        static int nBringIntoViewAttempts = -1;

        static TreeViewItem m_tvife
        {
            get { return m_tvife_; }
            set
            {
                m_tvife_ = value;

                m_tvivm_ = (m_tvife_ != null) ? (m_tvife_.DataContext as TreeViewItemVM) : null;
            }
        }
        static TreeViewItem m_tvife_ = null;
        static TreeViewItemVM m_tvivm { get { return m_tvivm_; } } static TreeViewItemVM m_tvivm_ = null;
    }

    public class TreeViewItemVM : ObservableObject
    {
        public ObservableCollection<TreeViewItemVM> Items { get { return m_Items; } }
        public string Text { get { return datum.Text.PadRight(200); } }
#if (WPF)
        public Brush Foreground { get { return m_bSelected ? m_SelectedForeground : m_Foreground; } }
        public Brush SelectedForeground { get { return m_bSelected ? m_SelectedForeground : m_Foreground; } }
        public Brush Background { get { return SDLWPF._BackClrToBrush(datum.BackColor); } }
        public FontWeight FontWeight { get { return m_bSelected ? FontWeights.ExtraBold : FontWeights.Normal; } }
#else
        public Brush Foreground;
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

                m_bSelected = value;
                EphemeralExpandedPos = -1;
                TVI_DependencyProperty.SetWaitingToSelect(null);
                m_SelectedForeground = Brushes.White;
                Utilities.WriteLine("IsSelected " + Text);

                if (TVI_DependencyProperty.WaitingToSelect != null)
                {
                    return;     // hack: Why does expanding a node select it?
                }

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

            m_bSelected = bSelect;
            RaisePropertyChanged("IsSelected");

            Utilities.WriteLine("SelectProgrammatic");
            TVI_DependencyProperty.SetWaitingToSelect(null);

            if (bSelect == false)
            {
                EphemeralExpandedPos = -1;
                return;
            }

            TVVM.SelectedItem = this;

            Stack<TreeViewItemVM> stackParents = new Stack<TreeViewItemVM>(8);
            TVI_DependencyProperty.stackParents = new Stack<TreeViewItemVM>(8);
            UList<TreeViewItemVM> listParents = new UList<TreeViewItemVM>();
            TreeViewItemVM parentItem = m_Parent;

            while (parentItem != null)
            {
                stackParents.Push(parentItem);
                TVI_DependencyProperty.stackParents.Push(parentItem);
                listParents.Add(parentItem);
                parentItem = parentItem.m_Parent;
            }

            foreach (TreeViewItemVM tvivm in TVVM.m_listExpanded.ToArray())
            {
                if ((stackParents.Contains(tvivm) == false) && tvivm.m_bExpanded)
                {
                    tvivm.EphemeralExpandedPos = -1;
                    tvivm.m_bExpanded = false;
                    tvivm.RaisePropertyChanged("IsExpanded");
                    TVVM.m_listExpanded.Remove(tvivm);
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

            if (TVI_DependencyProperty.tvivmSelected == this)
            {
                //      Utilities.WriteLine("tvivmSelected == " + Text);
                return;
            }

            EphemeralExpandedPos += (Index + 1);
            EphemeralExpandedPos *= HeaderHeight;       // when implementing variable-height headers this calc will be wrong
            TVVM.m_listExpanded = listParents;
            TVI_DependencyProperty.SetWaitingToSelect(this);
        }

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
            m_Foreground = SDLWPF._ForeClrToBrush(datum.ForeColor);

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
        internal Brush m_SelectedForeground = Brushes.White;
        readonly Brush m_Foreground = Brushes.DarkRed;

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
    }
}
