using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    internal class TreeViewItem_FileHashVM : ObservableObjectBase
    {
        internal readonly TreeViewItem_FileHashVM m_Parent = null;
        internal readonly TreeView_FileHashVM TVVM = null;
        internal double EphemeralExpandedPos = -1;
        internal Brush m_SelectedForeground = Brushes.White;
        internal readonly LocalTreeNode datum = null;
        internal int Index = -1;

        public ObservableCollection<TreeViewItem_FileHashVM> Items { get { return m_Items; } }
        public string Text { get { return datum.Text.PadRight(200); } }
        public Brush Foreground { get { return m_bSelected ? m_SelectedForeground : FrontBrush; } }
        public Brush SelectedForeground { get { return m_bSelected ? m_SelectedForeground : FrontBrush; } }
        public Brush Background { get { return UtilColor.ARGBtoBrush(datum.BackColor); } }
        public FontWeight FontWeight { get { return m_bSelected ? FontWeights.ExtraBold : FontWeights.Normal; } }
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
                UtilProject.WriteLine("IsSelected " + Text);

                if (TVI_DependencyProperty.WaitingToSelect != null)
                {
                    return;     // hack: Why does expanding a node select it?
                }

                if (datum.LVIVM != null)
                {
                    //if (m_bSelected)
                    //{
                    //    datum.LVIVM.LVVM.LVFE.ScrollIntoView(datum.LVIVM.LVVM.LVFE.Items[datum.LVIVM.Index]);
                    //}

                    //datum.LVIVM.SelectProgrammatic(m_bSelected);
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

            UtilProject.WriteLine("SelectProgrammatic");
            TVI_DependencyProperty.SetWaitingToSelect(null);

            if (bSelect == false)
            {
                EphemeralExpandedPos = -1;
                return;
            }

            TVVM.SelectedItem = this;

            var stackParents = new Stack<TreeViewItem_FileHashVM>(8);
            TVI_DependencyProperty.stackParents = new Stack<TreeViewItem_FileHashVM>(8);
            var listParents = new UList<TreeViewItem_FileHashVM>();
            var parentItem = m_Parent;

            while (parentItem != null)
            {
                stackParents.Push(parentItem);
                TVI_DependencyProperty.stackParents.Push(parentItem);
                listParents.Add(parentItem);
                parentItem = parentItem.m_Parent;
            }

            foreach (var tvivm in TVVM.m_listExpanded.ToArray()
                .Where(tvivm => (stackParents.Contains(tvivm) == false) &&
                    tvivm.m_bExpanded))
            {
                tvivm.EphemeralExpandedPos = -1;
                tvivm.m_bExpanded = false;
                tvivm.RaisePropertyChanged("IsExpanded");
                TVVM.m_listExpanded.Remove(tvivm);
            }

            while (false == stackParents.IsEmpty())
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
                //      UtilProject.WriteLine("tvivmSelected == " + Text);
                return;
            }

            EphemeralExpandedPos += (Index + 1);
            EphemeralExpandedPos *= HeaderHeight;       // when implementing variable-height headers this calc will be wrong
            TVVM.m_listExpanded = listParents;
            TVI_DependencyProperty.SetWaitingToSelect(this);
        }

        internal TreeViewItem_FileHashVM(TreeView_FileHashVM tvvm, LocalTreeNode datum_in, int nIndex)
            : this(tvvm, datum_in, null, nIndex)
        { }

        TreeViewItem_FileHashVM(TreeView_FileHashVM tvvm, LocalTreeNode datum_in,
            TreeViewItem_FileHashVM parent, int nIndex)
        {
            TVVM = tvvm;
            datum = datum_in;
            m_Parent = parent;
            Index = nIndex;
            datum.TVIVM = this;
         //   m_Foreground = SDLWPF._ForeClrToBrush(datum.ForeColor);

            var nIndex_A = -1;

            m_Items = new ObservableCollection<TreeViewItem_FileHashVM>
            (
                from item
                    in datum.Nodes.Keys
                    select new TreeViewItem_FileHashVM(tvvm, item, this, ++nIndex_A)
            );
        }

        readonly ObservableCollection<TreeViewItem_FileHashVM> m_Items = null;

        static double HeaderHeight { get { return TVI_DependencyProperty.HeaderHeight; } }
        Brush FrontBrush
        {
            get
            {
                return UtilColor.ARGBtoBrush(
                    (datum.ForeColor == UtilColor.Empty) ? UtilColor.DarkRed : datum.ForeColor
                );
            }
        }

        bool m_bExpanded = false;
        bool m_bSelected = false;
    }
}
