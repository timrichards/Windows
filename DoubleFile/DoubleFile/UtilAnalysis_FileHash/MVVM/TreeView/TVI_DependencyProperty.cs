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
        static public readonly DependencyProperty EventProperty = DependencyProperty.RegisterAttached
        ("Event", typeof(bool), typeof(TVI_DependencyProperty), new UIPropertyMetadata(false, OnDPchanged));

        static public bool GetEvent(FrameworkElement element) { return (bool)element.GetValue(EventProperty); }
        static public void SetEvent(FrameworkElement element, bool value) { element.SetValue(EventProperty, value); }

        static internal TreeView TVFE
        {
            get { return _TVFE; }
            set
            {
                _TVFE = value;

                if (m_scrollTimer != null)
                {
                    m_scrollTimer.Enabled = false;
                }

                //     m_scrollTimer = new SDL_Timer(1, OnTimer);    // 1 evil: static and not disposed. Works otherwise.
                StopTimer(2);
            }
        }
        static TreeView _TVFE = null;

        static internal void StartTimer(string text) { } // m_scrollTimer.Stop(); m_scrollTimer.Start(); UtilProject.WriteLine("StartTimer " + text); }
        static internal void StopTimer(int nWhere) { }   // m_scrollTimer.Stop(); UtilProject.WriteLine("StopTimer " + nWhere); }
        static internal TreeViewItem_FileHashVM WaitingToSelect { get; private set; }
        static internal void SetWaitingToSelect(TreeViewItem_FileHashVM tvivm)
        {
            WaitingToSelect = tvivm;

            if (WaitingToSelect != null)
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

        static internal ScrollViewer ScrollViewer
        {
            get { return _scrollViewer ?? (_scrollViewer = TVFE.Template.FindName("_tv_scrollviewer_", TVFE) as ScrollViewer); }
        }
        static ScrollViewer _scrollViewer = null;

        static internal TreeViewItem_FileHashVM tvivmSelected = null;
        static internal double HeaderHeight = -1;
        static internal Stack<TreeViewItem_FileHashVM> stackParents = null;

        static internal void OnTimer()
        {
            StopTimer(1);

            if (nBringIntoViewAttempts >= 0)
            {
                if (TVIFE == null)
                {
                    UtilProject.WriteLine("OnTimer m_tvife == null");
                    return;
                }

                var tvivm = m_TVIVM;

                if (tvivm == null)      // DisconnectedItem
                {
                    UtilProject.WriteLine("OnTimer tvivm == null");
                    return;
                }

                var ptTVI = TVIFE.TranslatePoint(new Point(0, 0), ScrollViewer);

                if ((ptTVI.Y >= 0) &&
                    (ptTVI.Y < ScrollViewer.ViewportHeight))
                {
                    UtilProject.WriteLine("ViewportHeight " + nBringIntoViewAttempts + " " + tvivm.Text);
                    tvivmSelected = tvivm;
                    nBringIntoViewAttempts = -1;
                    TVIFE = null;
                    return;
                }

                if (nBringIntoViewAttempts < 20)
                {
                    nBringIntoViewAttempts += 20;
                }

                switch (nBringIntoViewAttempts)
                {
                    case 20:
                        ScrollViewer.ScrollToTop();
                        break;

                    case 21:
                        TVIFE.BringIntoView();
                        break;

                    case 22:
                        ScrollViewer.PageUp();
                        break;

                    case 23:
                        TVIFE.BringIntoView();
                        break;

                    case 24:
                        ScrollViewer.PageDown();
                        break;

                    case 25:
                        ScrollViewer.PageDown();
                        break;

                    case 26:
                        TVIFE.BringIntoView();
                        break;

                    case 27:
                    default:
                        UtilProject.WriteLine("default " + tvivm.Text);
                        TVIFE = null;
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

            if (false == stackParents.IsEmpty())
            {
                tvivm_A = stackParents.Pop();
            }
            else
            {
                if (new[] { WaitingToSelect.EphemeralExpandedPos, 0, ScrollViewer.ScrollableHeight }
                    .Contains(ScrollViewer.VerticalOffset))
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

        // This is where you modify (a) the type; and (b) the event handled.
        static void OnDPchanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var tvife = depObj as TreeViewItem;
            var bAddEvt = (bool)e.NewValue;

            if (tvife == null)
            {
                return;
            }

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

            if (false == stackParents.IsEmpty())
            {
                Scroll(stackParents.Pop());
            }
        }

        static void OnLoaded(object sender, EventArgs e)
        {
            if (HeaderHeight > 0)
            {
                return;
            }

            var tvife = sender as TreeViewItem;

            if (tvife == null)
            {
                return;
            }

            var header = tvife.Template.FindName("PART_Header", tvife) as FrameworkElement;

            if (header != null)
            {
                HeaderHeight = header.ActualHeight;
            }
        }

        static void OnDeselect(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(sender, e.OriginalSource))
            {
                var tvife = sender as TreeViewItem;

                if (tvife == null)
                {
                    return;
                }

                var tvivm = tvife.DataContext as TreeViewItem_FileHashVM;

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
            if (ReferenceEquals(sender, e.OriginalSource) == false)
            {
            }

            var tvife = sender as TreeViewItem;

            if (tvife == null)
            {
                return;
            }

            var tvivm = tvife.DataContext as TreeViewItem_FileHashVM;

            if (tvivm == null)      // DisconnectedItem
            {
                tvife.BringIntoView();
                return;
            }

            var waitingToSelect = WaitingToSelect;

            if (tvivm == waitingToSelect)   // LV programmatic
            {
                tvife.BringIntoView();
                SetWaitingToSelect(null);
                nBringIntoViewAttempts = -1;

                var ptTVI = tvife.TranslatePoint(new Point(0, 0), ScrollViewer);

                if ((ptTVI.Y >= 0) &&
                    (ptTVI.Y < ScrollViewer.ViewportHeight))
                {
                    UtilProject.WriteLine("ViewportHeight A " + tvivm.Text);
                    tvivmSelected = tvivm;
                    nBringIntoViewAttempts = -1;
                    TVIFE = null;
                    return;
                }

                tvivm.m_SelectedForeground = tvivm.Foreground;
                UtilProject.WriteLine("Set nBringIntoViewAttempts " + tvivm.Text);
                TVIFE = tvife;
                nBringIntoViewAttempts = 0;
                StartTimer("5 " + tvivm.Text);
            }
            else if (false == ReferenceEquals(sender, e.OriginalSource))
            {
                return;
            }
            else    // direct treeview selection
            {
                SetWaitingToSelect(null);
                TVIFE = null;
                nBringIntoViewAttempts = -1;
                tvivm.m_SelectedForeground = Brushes.White;
            }

            tvivm.RaisePropertyChanged("Foreground");
            tvivm.RaisePropertyChanged("SelectedForeground");
            tvivm.RaisePropertyChanged("FontWeight");
        }

        static void Scroll(TreeViewItem_FileHashVM tvivm)
        {
            StopTimer(3);
            UtilProject.WriteLine(tvivm.datum.Level + " " + tvivm.Text + " " + tvivm.EphemeralExpandedPos);
            ScrollViewer.ScrollToTop();
            ScrollViewer.ScrollToVerticalOffset(tvivm.EphemeralExpandedPos);
            StartTimer("3 " + tvivm.Text);
        }

        static Stack<TreeViewItem_FileHashVM> stackParents_A = null;
        static readonly SDL_Timer m_scrollTimer = null;
        static int nAttempts = -1;
        static int nBringIntoViewAttempts = -1;

        static TreeViewItem TVIFE
        {
            get { return _tvife; }
            set
            {
                _tvife = value;

                m_TVIVM = (_tvife != null) ? (_tvife.DataContext as TreeViewItem_FileHashVM) : null;
            }
        }
        static TreeViewItem _tvife = null;

        static TreeViewItem_FileHashVM m_TVIVM = null;
    }
}
