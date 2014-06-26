using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace SearchDirLists
{
    public static class LVI_DependencyProperty
    {
        public static readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached
        ("Event", typeof(bool), typeof(LVI_DependencyProperty), new UIPropertyMetadata(false, OnDPchanged));

        public static bool GetEvent(FrameworkElement element) { return (bool)element.GetValue(EventProperty); }
        public static void SetEvent(FrameworkElement element, bool value) { element.SetValue(EventProperty, value); }

        // This is where you modify (a) the type; and (b) the event handled.
        static void OnDPchanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            ListViewItem lvife = depObj as ListViewItem;
            bool bAddEvt = (bool)e.NewValue;

            if (bAddEvt)
            {
                lvife.KeyUp += OnKeyUp;
                lvife.MouseUp += OnMouseUp;
            }
            else
            {
                lvife.KeyUp -= OnKeyUp;
                lvife.MouseUp -= OnMouseUp;
            }
        }

        static void OnKeyUp(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource))
            {
                ((ListViewItemVM)((ListViewItem)sender).DataContext).KeyUp((KeyEventArgs)e);
            }
        }

        static void OnMouseUp(object sender, RoutedEventArgs e)
        {
            if (Object.ReferenceEquals(sender, e.OriginalSource))
            {
                ((ListViewItemVM)((ListViewItem)sender).DataContext).MouseUp();
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
