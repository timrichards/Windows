using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;

namespace DoubleFile
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

                TreeViewItem_FileHashVM tvivm = tvife.DataContext as TreeViewItem_FileHashVM;

                if (tvivm == null)      // DisconnectedItem
                {
                    return;
                }

                tvivm.m_bBringIntoViewWhenSel = false;
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

            WaitingToSelect = null;
            m_tvife = sender as TreeViewItem;

            if (m_tvife == null)
            {
                return;
            }

            TreeViewItem_FileHashVM tvivm = m_tvife.DataContext as TreeViewItem_FileHashVM;

            if (tvivm == null)      // DisconnectedItem
            {
                return;
            }

            if (tvivm.m_bBringIntoViewWhenSel || (tvivm.IsExpanded == false))
            {
                tvivm.m_SelectedForeground = tvivm.Foreground;
                scrollViewer.PageDown();
                m_tvife.BringIntoView();

                Point ptTVI = m_tvife.TranslatePoint(new Point(0, 0), scrollViewer);

                if ((ptTVI.Y >= 0) && (ptTVI.Y < scrollViewer.ViewportHeight))
                {
                    tvivmSelected = tvivm;
                }
                else
                {
                    nBringIntoViewAttempts = 0;
                    m_scrollTimer.Start();
                }
            }
            else
            {
                tvivm.m_SelectedForeground = Brushes.White;
            }

            tvivm.RaisePropertyChanged("Foreground");
            tvivm.RaisePropertyChanged("SelectedForeground");
            tvivm.RaisePropertyChanged("FontWeight");
        }

        internal static void OnTimer(object o, EventArgs e)
        {
            if (nBringIntoViewAttempts >= 0)
            {
                m_scrollTimer.Stop();

                if (m_tvife == null)
                {
                    return;
                }

                if (m_tvife.IsVisible)
                {
                    tvivmSelected = (TreeViewItem_FileHashVM)m_tvife.DataContext;
                    nBringIntoViewAttempts = -1;
                    m_tvife = null;
                    return;
                }

                switch (nBringIntoViewAttempts)
                {
                    case 0:
                        scrollViewer.PageUp();
                        break;

                    case 1:
                        m_tvife.BringIntoView();
                        break;

                    case 2:
                        scrollViewer.PageDown();
                        break;

                    case 3:
                        scrollViewer.PageDown();
                        break;

                    case 4:
                        m_tvife.BringIntoView();
                        break;

                    case 5:
                    default:
                        m_tvife = null;
                        nBringIntoViewAttempts = -1;
                        return;
                }

                ++nBringIntoViewAttempts;
                m_scrollTimer.Start();
                return;
            }

            if (WaitingToSelect == null)
            {
                return;
            }

            TreeViewItem_FileHashVM tvivm = null;

            if (stackParents.Count > 0)
            {
                tvivm = stackParents.Pop();
            }
            else
            {
                if (new double[] { WaitingToSelect.EphemeralExpandedPos, 0, scrollViewer.ScrollableHeight }.Contains(scrollViewer.VerticalOffset))
                {
                    if (++nAttempts > 3)
                    {
                        WaitingToSelect = null;
                        return;
                    }

                    stackParents = stackParents_A;
                }

                tvivm = WaitingToSelect;
            }

            Scroll(tvivm);
        }

        static void Scroll(TreeViewItem_FileHashVM tvivm)
        {
            m_scrollTimer.Stop();
            //Utilities.WriteLine(tvivm.datum.Level + " " + tvivm.Text + " " + tvivm.EphemeralExpandedPos);
            scrollViewer.ScrollToTop();
            scrollViewer.ScrollToVerticalOffset(tvivm.EphemeralExpandedPos);
            m_scrollTimer.Start();
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

                m_scrollTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(20), DispatcherPriority.Normal, new EventHandler(TVI_DependencyProperty.OnTimer), TVFE.Dispatcher);
                m_scrollTimer.Stop();
            }
        }
        static TreeView m_TVFE = null;

        internal static TreeViewItem_FileHashVM WaitingToSelect
        {
            get { return m_WaitingToSelect; }
            set
            {
                m_WaitingToSelect = value;

                if (m_WaitingToSelect != null)
                {
                    stackParents_A = new Stack<TreeViewItem_FileHashVM>(stackParents.Reverse());
                    m_scrollTimer.Start();
                }
                else
                {
                    stackParents = stackParents_A = new Stack<TreeViewItem_FileHashVM>(8);
                    nAttempts = -1;
                    m_scrollTimer.Stop();
                }
            }
        }
        static TreeViewItem_FileHashVM m_WaitingToSelect = null;

        internal static ScrollViewer scrollViewer
        {
            get { return m_scrollViewer_ ?? (m_scrollViewer_ = TVFE.Template.FindName("_tv_scrollviewer_", TVFE) as ScrollViewer); }
        }
        static ScrollViewer m_scrollViewer_ = null;

        internal static TreeViewItem_FileHashVM tvivmSelected = null;
        internal static double HeaderHeight = -1;
        internal static Stack<TreeViewItem_FileHashVM> stackParents = null;
        static Stack<TreeViewItem_FileHashVM> stackParents_A = null;
        static DispatcherTimer m_scrollTimer = null;
        static int nAttempts = -1;
        static int nBringIntoViewAttempts = -1;
        static TreeViewItem m_tvife = null;
    }
}
