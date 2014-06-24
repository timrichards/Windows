using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace SearchDirLists
{
    public static class LVI_DP_KeyUp
    {
        public static readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached
        ("Event", typeof(bool), typeof(LVI_DP_KeyUp), new UIPropertyMetadata(false, OnDPchanged));

        public static bool GetEvent(FrameworkElement element) { return (bool)element.GetValue(EventProperty); }
        public static void SetEvent(FrameworkElement element, bool value) { element.SetValue(EventProperty, value); }

        static void OnDPchanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        // This is where you modify (a) the type; and (b) the event handled.
        { ListViewItem item = depObj as ListViewItem; if ((bool)e.NewValue) { item.KeyUp += OnEvent; } else { item.KeyUp -= OnEvent; } }

        static void OnEvent(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource))
            {
                ((ListViewItemVM)((ListViewItem)sender).DataContext).KeyUp((KeyEventArgs)e);
            }
            else
            {
                //Utilities.WriteLine("Not original source: " + sender + " != " + e.OriginalSource + ". Source = " + e.Source);
            }
        }
    }

    public static class LVI_DP_MouseUp
    {
        public static readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached
        ("Event", typeof(bool), typeof(LVI_DP_MouseUp), new UIPropertyMetadata(false, OnDPchanged));

        public static bool GetEvent(FrameworkElement element) { return (bool)element.GetValue(EventProperty); }
        public static void SetEvent(FrameworkElement element, bool value) { element.SetValue(EventProperty, value); }

        static void OnDPchanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        // This is where you modify (a) the type; and (b) the event handled.
        { ListViewItem item = depObj as ListViewItem; if ((bool)e.NewValue) { item.MouseUp += OnEvent; } else { item.MouseUp -= OnEvent; } }

        static void OnEvent(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource))
            {
                ((ListViewItemVM)((ListViewItem)sender).DataContext).MouseUp();
            }
            else
            {
                //Utilities.WriteLine("Not original source: " + sender + " != " + e.OriginalSource + ". Source = " + e.Source);
            }
        }
    }

    // Used for two listviewers
    public class ClonesLVitemVM : ClonesLVitemVM_Base
    {
        public String Occurrences { get { return datum.SubItems[1].Text; } }
        readonly static String[] marrPropName = new String[] { "Folders", "Occurrences" };
        internal const int NumCols_ = 2;

        internal ClonesLVitemVM(ListViewVM LV, SDL_ListViewItem datum_in)
            : base(LV, datum_in)
        {
            if (datum.Tag == null)
            {
                return;     // marker item
            }

            foreach (SDL_TreeNode treeNode in ((UList<SDL_TreeNode>)datum.Tag))
            {
                if (treeNode.LVIVM == null)        // TODO: take same vol items out of the clones LV?
                {
                    treeNode.LVIVM = this;
                }
            }
        }

        internal override void KeyUp(KeyEventArgs e)
        {
            if ((e.Key == Key.Left) && (m_nClonesIx > 0))
            {
                m_nClonesIx -= 2;
            }
            else if (e.Key != Key.Right)
            {
                return;
            }

            SelectNextTreeNode();
            e.Handled = true;
        }

        internal override void MouseUp()
        {
            if (m_bSelChange == false)
            {
                SelectNextTreeNode();
            }

            m_bSelChange = false;
        }

        internal override int NumCols { get { return NumCols_; } }
        protected override String[] PropertyNames { get { return marrPropName; } }

        protected override void ActOnDirectSelChange()
        {
            m_nClonesIx = -1;
            m_bSelChange = true;
            SelectNextTreeNode();
        }

        void SelectNextTreeNode()
        {
            UList<SDL_TreeNode> listNodes = ((UList<SDL_TreeNode>)datum.Tag);

            if (listNodes == null)
            {
                return;     // marker item
            }

            bool bSelected = m_bSelected;

            Action go = new Action(() =>
            {
                (listNodes[m_nClonesIx %= listNodes.Count]).TVIVM.SelectProgrammatic(bSelected);
            });

            if (m_nClonesIx >= 0)
            {
                bSelected = false;
                go();
                bSelected = true;
            }

            ++m_nClonesIx;
            go();
        }

        int m_nClonesIx = -1;
        bool m_bSelChange = false;
    }

    public class ClonesListViewVM : ListViewVM_Generic<ClonesLVitemVM>
    {
        public String WidthFolders { get { return SCW; } }          // not used
        public String WidthOccurrences { get { return SCW; } }      // not used

        internal ClonesListViewVM(ListView lv) : base(lv) { }
        internal override void NewItem(SDL_ListViewItem datum_in, bool bQuiet = false) { Add(new ClonesLVitemVM(this, datum_in), bQuiet); }
        internal override int NumCols { get { return ClonesLVitemVM.NumCols_; } }
    }
}
