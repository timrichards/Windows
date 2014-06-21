using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace SearchDirLists
{
    public static partial class SDL_DependencyProperties
    {
        public static bool GetViewSel(TreeViewItem treeViewItem)
        {
            return (bool)treeViewItem.GetValue(ViewSelProperty);
        }

        public static void SetViewSel(TreeViewItem treeViewItem, bool value)
        {
            treeViewItem.SetValue(ViewSelProperty, value);
        }

        public static readonly DependencyProperty ViewSelProperty = DependencyProperty.RegisterAttached
        (
            "ViewSel",
            typeof(bool),
            typeof(SDL_DependencyProperties),
            new UIPropertyMetadata(false, OnViewSelChanged)
        );

        static void OnViewSelChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = depObj as TreeViewItem;

            if (item == null)
            {
                return;
            }

            if (e.NewValue is bool)
            {
                if ((bool)e.NewValue)
                {
                    item.Selected += OnTreeViewItemSelected;
                }
                else
                {
                    item.Selected -= OnTreeViewItemSelected;
                }
            }
        }

        static void OnTreeViewItemSelected(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource))
            {
                TreeViewItem item = e.OriginalSource as TreeViewItem;

                if (item != null)
                {
                    item.BringIntoView();
                }
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
                    m_bExpanded = ExpandParents(value);
                    RaisePropertyChanged("IsExpanded");
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
                    m_bSelected = ExpandParents(value);
                    RaisePropertyChanged("IsSelected");

                    if (m_bSelected)
                    {
                        TV.m_SelectedItem = this;
                    }
                    else if (TV.m_SelectedItem == this)
                    {
                        TV.m_SelectedItem = null;
                    }
                }
            }
        }

        bool ExpandParents(bool bExpand)
        {
            if (bExpand)
            {
                Stack<TreeViewItemVM> stackItems = new Stack<TreeViewItemVM>(8);
                TreeViewItemVM parentItem = m_Parent;

                while (parentItem != null)
                {
                    stackItems.Push(parentItem);
                    parentItem = parentItem.m_Parent;
                }

                while (stackItems.Count > 0)
                {
                    parentItem = stackItems.Pop();
                    parentItem.IsExpanded = true;
                }
            }

            return bExpand;
        }

        internal TreeViewItemVM(TreeViewVM tv, SDL_TreeNode datum_in)
            : this(tv, datum_in, null)
        { }

        TreeViewItemVM(TreeViewVM tv, SDL_TreeNode datum_in, TreeViewItemVM parent)
        {
            TV = tv;
            datum = datum_in;
            m_Parent = parent;
#if (WPF)
            datum.VM = this;

            m_Items = new ObservableCollection<TreeViewItemVM>
            (
                (from item in datum.Nodes.Keys select new TreeViewItemVM(tv, item, this))
                .ToList<TreeViewItemVM>()
            );
#endif
        }

        readonly ObservableCollection<TreeViewItemVM> m_Items = null;
        readonly TreeViewItemVM m_Parent = null;
        readonly SDL_TreeNode datum = null;

        bool m_bExpanded = false;
        bool m_bSelected = false;
        TreeViewVM TV = null;
    }

    class TreeViewVM
    {
        internal void SetData(TreeView tv, List<SDL_TreeNode> rootNodes)
        {
            foreach (SDL_TreeNode treeNode in rootNodes)
            {
                m_Items.Add(new TreeViewItemVM(this, treeNode));
            }

            tv.DataContext = m_Items;
        }

        internal TreeViewItemVM m_SelectedItem = null;
        readonly List<TreeViewItemVM> m_Items = new List<TreeViewItemVM>();
    }
}
