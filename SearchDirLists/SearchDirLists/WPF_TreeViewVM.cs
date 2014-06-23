using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace SearchDirLists
{
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
                        TV.m_listExpanded.Add(this);
                    }
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
                TV.ScrollToHome();
            }

            m_bSelected = bSelect;
            RaisePropertyChanged("IsSelected");

            if (bSelect == false)
            {
                return;
            }

            TV.SelectedItem = this;

            Stack<TreeViewItemVM> stackParents = new Stack<TreeViewItemVM>(8);
            UList<TreeViewItemVM> listParents = new UList<TreeViewItemVM>();
            TreeViewItemVM parentItem = m_Parent;

            while (parentItem != null)
            {
                stackParents.Push(parentItem);
                listParents.Add(parentItem);
                parentItem = parentItem.m_Parent;
            }

            foreach (TreeViewItemVM item in TV.m_listExpanded)
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

            TV.m_listExpanded = listParents;
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
                        TV.SelectedItem = this;
                    }
                    else if (TV.SelectedItem == this)
                    {
                        TV.SelectedItem = null;
                    }

//                    Utilities.WriteLine("End IsSelected " + Text + " " + value);
                }
            }
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
            datum.TVIVM = this;

            m_Items = new ObservableCollection<TreeViewItemVM>
            (
                (from item in datum.Nodes.Keys select new TreeViewItemVM(tv, item, this))
                .ToList<TreeViewItemVM>()
            );
#endif
        }

        readonly ObservableCollection<TreeViewItemVM> m_Items = null;
        readonly TreeViewItemVM m_Parent = null;
        readonly TreeViewVM TV = null;

        readonly SDL_TreeNode datum = null;

        bool m_bExpanded = false;
        bool m_bSelected = false;
    }

    class TreeViewVM
    {
        internal void ScrollToHome()
        {
            m_ScrollViewer.ScrollToHome();
        }

        internal void SetData(TreeView tv, List<SDL_TreeNode> rootNodes)
        {
            foreach (SDL_TreeNode treeNode in rootNodes)
            {
                m_Items.Add(new TreeViewItemVM(this, treeNode));
            }

            m_ScrollViewer = tv.Template.FindName("_tv_scrollviewer_", tv) as ScrollViewer;            
            tv.DataContext = m_Items;
        }

        internal TreeViewItemVM SelectedItem = null;
        internal UList<TreeViewItemVM> m_listExpanded = new UList<TreeViewItemVM>();
        
        readonly List<TreeViewItemVM> m_Items = new List<TreeViewItemVM>();
        ScrollViewer m_ScrollViewer = null;

        internal DateTime mDbg_dtProgrammaticExpand = DateTime.MinValue;
    }
}
