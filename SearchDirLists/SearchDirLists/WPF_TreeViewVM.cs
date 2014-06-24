using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows;

namespace SearchDirLists
{
    public static class TVI_DP_Selected     // This could all go into IsSelected if tvivm knew its tvi.
    {
        public static readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached
        ("Event", typeof(bool), typeof(TVI_DP_Selected), new UIPropertyMetadata(false, OnDPchanged));

        public static bool GetEvent(FrameworkElement element) { return (bool)element.GetValue(EventProperty); }
        public static void SetEvent(FrameworkElement element, bool value) { element.SetValue(EventProperty, value); }

        static void OnDPchanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        // This is where you modify (a) the type; and (b) the event handled.
        { TreeViewItem item = depObj as TreeViewItem; if ((bool)e.NewValue) { item.Selected += OnEvent; } else { item.Selected -= OnEvent; } }

        static void OnEvent(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource))
            {
                TreeViewItem tvi = e.OriginalSource as TreeViewItem;
                TreeViewItemVM tvivm = (TreeViewItemVM)tvi.DataContext;

                if (tvivm.m_bProgrammaticSelect == false)
                {
                    Utilities.WriteLine(DateTime.Now + " tvivm.m_bProgrammaticSelect == false");
                    return;
                }

                ScrollViewer scrollViewer = tvivm.TVVM.TVFE.Template.FindName("_tv_scrollviewer_", tvivm.TVVM.TVFE) as ScrollViewer;

                if (scrollViewer == null)
                {
                    Utilities.WriteLine(DateTime.Now + " scrollViewer == null");
                    return;
                }

                Point relativePosition = tvi.TranslatePoint(new Point(0, 0), scrollViewer);

                tvi.BringIntoView();
                scrollViewer.ScrollToVerticalOffset(relativePosition.Y);
                tvivm.m_bProgrammaticSelect = false;
            }
            else
            {
                //Utilities.WriteLine("Not original source: " + sender + " != " + e.OriginalSource + ". Source = " + e.Source);
            }
        }
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
                if (value != m_bExpanded)
                {
//                    Utilities.WriteLine("Set IsExpanded " + Text + " " + value);
                    m_bExpanded = value;

                    if (m_bExpanded)
                    {
                        TVVM.m_listExpanded.Add(this);
                    }
                }
            }
        }

        public bool IsSelected
        {
            get { return m_bSelected; }
            set
            {
                if (value != m_bSelected)
                {
#if (DEBUG && false)
                    if (DateTime.Now - TV.mDbg_dtProgrammaticExpand > TimeSpan.FromMilliseconds(50))
                    {
                        Utilities.WriteLine("-");
                        TV.mDbg_dtProgrammaticExpand = DateTime.Now;
                    }

                    Utilities.WriteLine("Set IsSelected " + Text + " " + value);
#endif
                    m_bSelected = value;

                    if (datum.LVIVM != null)
                    {
                        datum.LVIVM.LVVM.LVFE.ScrollIntoView(datum.LVIVM.LVVM.LVFE.Items[datum.LVIVM.Index]);
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

//                    Utilities.WriteLine("End IsSelected " + Text + " " + value);
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
                //           TVVM.ScrollToHome();
            }

            m_bProgrammaticSelect = true;
            m_bSelected = bSelect;
            RaisePropertyChanged("IsSelected");

            if (bSelect == false)
            {
                return;
            }

            TVVM.SelectedItem = this;

            Stack<TreeViewItemVM> stackParents = new Stack<TreeViewItemVM>(8);
            UList<TreeViewItemVM> listParents = new UList<TreeViewItemVM>();
            TreeViewItemVM parentItem = m_Parent;

            while (parentItem != null)
            {
                stackParents.Push(parentItem);
                listParents.Add(parentItem);
                parentItem = parentItem.m_Parent;
            }

            foreach (TreeViewItemVM item in TVVM.m_listExpanded)
            {
                if (stackParents.Contains(item) == false)
                {
                    item.m_bExpanded = false;
                    item.RaisePropertyChanged("IsExpanded");
                }
            }

            while (stackParents.Count > 0)
            {
                parentItem = stackParents.Pop();

                if (parentItem.m_bExpanded == false)
                {
                    parentItem.m_bExpanded = true;
                    parentItem.RaisePropertyChanged("IsExpanded");
                }
            }

            TVVM.m_listExpanded = listParents;
        }

        internal bool m_bProgrammaticSelect = false;

        internal TreeViewItemVM(TreeViewVM tv, SDL_TreeNode datum_in)
            : this(tv, datum_in, null)
        { }

        TreeViewItemVM(TreeViewVM tv, SDL_TreeNode datum_in, TreeViewItemVM parent)
        {
            TVVM = tv;
            datum = datum_in;
            m_Parent = parent;
#if (WPF)
            datum.TVIVM = this;
            Index = tv.TVFE.Items.Count;

            m_Items = new ObservableCollection<TreeViewItemVM>
            (
                (from item in datum.Nodes.Keys select new TreeViewItemVM(tv, item, this))
                .ToList<TreeViewItemVM>()
            );
#endif
        }

        readonly ObservableCollection<TreeViewItemVM> m_Items = null;
        readonly TreeViewItemVM m_Parent = null;
        internal readonly TreeViewVM TVVM = null;

        readonly SDL_TreeNode datum = null;

        bool m_bExpanded = false;
        bool m_bSelected = false;
        int Index = -1;
    }

    class TreeViewVM
    {
        internal TreeViewVM(TreeView tvfe)
        {
            TVFE = tvfe;
        }

        //internal void ScrollToHome()
        //{
        //    m_ScrollViewer.ScrollToHome();
        //}

        internal void SetData(List<SDL_TreeNode> rootNodes)
        {
            foreach (SDL_TreeNode treeNode in rootNodes)
            {
                m_Items.Add(new TreeViewItemVM(this, treeNode));
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
