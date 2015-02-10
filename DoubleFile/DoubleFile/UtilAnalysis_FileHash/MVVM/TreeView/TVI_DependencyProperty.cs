using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

            TreeViewItem_FileHashVM tvivm = tvife.DataContext as TreeViewItem_FileHashVM;

            if (tvivm == null)      // DisconnectedItem
            {
                tvife.BringIntoView();
                return;
            }

            TreeViewItem_FileHashVM waitingToSelect = WaitingToSelect;

            if (tvivm == waitingToSelect)   // LV programmatic
            {
                tvife.BringIntoView();
                SetWaitingToSelect(null);
                nBringIntoViewAttempts = -1;

                System.Windows.Point ptTVI = tvife.TranslatePoint(new System.Windows.Point(0, 0), scrollViewer);

                if ((ptTVI.Y >= 0) && (ptTVI.Y < scrollViewer.ViewportHeight))
                {
                    UtilProject.WriteLine("ViewportHeight A " + tvivm.Text);
                    tvivmSelected = tvivm;
                    nBringIntoViewAttempts = -1;
                    m_tvife = null;
                    return;
                }

                tvivm.m_SelectedForeground = tvivm.Foreground;
                UtilProject.WriteLine("Set nBringIntoViewAttempts " + tvivm.Text);
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

        internal static void OnTimer()
        {
            StopTimer(1);

            if (nBringIntoViewAttempts >= 0)
            {
                if (m_tvife == null)
                {
                    UtilProject.WriteLine("OnTimer m_tvife == null");
                    return;
                }

                TreeViewItem_FileHashVM tvivm = m_tvivm;

                if (tvivm == null)      // DisconnectedItem
                {
                    UtilProject.WriteLine("OnTimer tvivm == null");
                    return;
                }

                System.Windows.Point ptTVI = m_tvife.TranslatePoint(new System.Windows.Point(0, 0), scrollViewer);

                if ((ptTVI.Y >= 0) && (ptTVI.Y < scrollViewer.ViewportHeight))
                {
                    UtilProject.WriteLine("ViewportHeight " + nBringIntoViewAttempts + " " + tvivm.Text);
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
                        UtilProject.WriteLine("default " + tvivm.Text);
                        m_tvife = null;
                        nBringIntoViewAttempts = -1;
                        return;
                }

                ++nBringIntoViewAttempts;
                StartTimer("1 " + tvivm.Text);
                UtilProject.WriteLine("nBringIntoViewAttempts " + nBringIntoViewAttempts + " " + tvivm.Text);
                return;
            }

            if (WaitingToSelect == null)
            {
                UtilProject.WriteLine("OnTimer WaitingToSelect == null");
                return;
            }

            TreeViewItem_FileHashVM tvivm_A = null;

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
                        UtilProject.WriteLine("OnTimer SetWaitingToSelect(null)");
                        return;
                    }

                    stackParents = stackParents_A;
                }

                tvivm_A = WaitingToSelect;
            }

            Scroll(tvivm_A);
            UtilProject.WriteLine("OnTimer end");
        }

        static void Scroll(TreeViewItem_FileHashVM tvivm)
        {
            StopTimer(3);
            UtilProject.WriteLine(tvivm.datum.Level + " " + tvivm.Text + " " + tvivm.EphemeralExpandedPos);
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
                    m_scrollTimer.Enabled = false;
                }

           //     m_scrollTimer = new SDL_Timer(1, OnTimer);    // 1 evil: static and not disposed. Works otherwise.
                StopTimer(2);
            }
        }
        static TreeView m_TVFE = null;

        static internal void StartTimer(string text) {} // m_scrollTimer.Stop(); m_scrollTimer.Start(); UtilProject.WriteLine("StartTimer " + text); }
        static internal void StopTimer(int nWhere) {}   // m_scrollTimer.Stop(); UtilProject.WriteLine("StopTimer " + nWhere); }
        internal static TreeViewItem_FileHashVM WaitingToSelect { get { return m_WaitingToSelect; } }
        internal static void SetWaitingToSelect(TreeViewItem_FileHashVM tvivm)
        {
            m_WaitingToSelect = tvivm;

            if (m_WaitingToSelect != null)
            {
                stackParents_A = new Stack<TreeViewItem_FileHashVM>(stackParents.Reverse());
                StartTimer("0 " + tvivm.Text);
            }
            else
            {
                stackParents = stackParents_A = new Stack<TreeViewItem_FileHashVM>(8);
                nAttempts = -1;
                StopTimer(0);
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
        static SDL_Timer m_scrollTimer = null;
        static int nAttempts = -1;
        static int nBringIntoViewAttempts = -1;

        static TreeViewItem m_tvife
        {
            get { return m_tvife_; }
            set
            {
                m_tvife_ = value;

                m_tvivm_ = (m_tvife_ != null) ? (m_tvife_.DataContext as TreeViewItem_FileHashVM) : null;
            }
        }
        static TreeViewItem m_tvife_ = null;
        static TreeViewItem_FileHashVM m_tvivm { get { return m_tvivm_; } } static TreeViewItem_FileHashVM m_tvivm_ = null;
    }
}
