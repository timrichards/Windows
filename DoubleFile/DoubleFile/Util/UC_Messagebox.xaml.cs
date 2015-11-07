using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DoubleFile
{
    public partial class UC_Messagebox : UC_ContentPresenter
    {
        static internal bool Showing = false;

        MessageBoxButton Buttons
        {
            set
            {
                _buttons = value;
                formBtn_OK.SetValue(Grid.ColumnProperty, _OK_ColumnProperty);
                formBtn_OK.Content = "OK";
                formBtn_Cancel.Content = "Cancel";
                formBtn_Cancel.Visibility = Visibility.Visible;

                switch (_buttons)
                {
                    case MessageBoxButton.OK:
                    {
                        formBtn_Cancel.Visibility = Visibility.Hidden;
                        formBtn_OK.SetValue(Grid.ColumnProperty, formBtn_Cancel.GetValue(Grid.ColumnProperty));
                        break;
                    }

                    case MessageBoxButton.YesNo:
                    {
                        formBtn_OK.Content = "Yes";
                        formBtn_Cancel.Content = "No";
                        break;
                    }

                    case MessageBoxButton.YesNoCancel:
                    {
                        Util.Assert(99943, false);
                        break;
                    }
                }
            }
        }
        MessageBoxButton _buttons = MessageBoxButton.OKCancel;
        object _OK_ColumnProperty = null;

        public UC_Messagebox()
        {
            InitializeComponent();
            form_Message.FontSize += 4;
            _OK_ColumnProperty = formBtn_OK.GetValue(Grid.ColumnProperty);
        }

        internal MessageBoxResult
            ShowMessagebox(string strMessage, string strTitle, MessageBoxButton? buttons)
        {
            _lsDisposable.Add(Observable.FromEventPattern(formBtn_OK, "Click")
                .LocalSubscribe(99754, x => BtnOK_Click()));

            _lsDisposable.Add(Observable.FromEventPattern(formBtn_Cancel, "Click")
                .LocalSubscribe(99753, x => Kill()));

            _lsDisposable.Add(Statics.EscKey.LocalSubscribe(99623, x => Kill()));

            //if (null != strTitle)
            //    Title = strTitle;

            Buttons = buttons ?? MessageBoxButton.OK;

            switch (_buttons)
            {
                case MessageBoxButton.OKCancel:
                case MessageBoxButton.YesNoCancel:
                {
                    _Result = MessageBoxResult.Cancel;
                    break;
                }

                case MessageBoxButton.OK:
                {
                    _Result = MessageBoxResult.OK;
                    break;
                }

                case MessageBoxButton.YesNo:
                {
                    _Result = MessageBoxResult.No;
                    break;
                }
            }

            var loc = 99627;

            if (Showing)
                return MessageBox.Show(strMessage + "\nMessagebox: there is a message already up", strTitle, buttons ?? MessageBoxButton.OK);

            Action show = () =>
            {
                try
                {
                    Showing = true;
                    form_Message.Text = strMessage;
                    LocalShow(loc);
                    _shown = this;
                    _dispatcherFrame.PushFrameTrue();
                }
                finally
                {
                    _shown = null;
                    LocalHide(loc);
                    Util.LocalDispose(_lsDisposable);
                    Showing = false;
                }
            };

            if (null == ProgressOverlay.WithProgressOverlay(w => ((IDimForMessagebox)w).Go(show)))
            {
                show();
                LocalHide(-1, bForce: true);
            }

            return _Result;
        }
        static UC_Messagebox _shown = null;
        void Kill_() => _dispatcherFrame.Continue = false;
        static internal void
            Kill() => _shown?.Kill_();

        void BtnOK_Click()
        {
            switch (_buttons)
            {
                case MessageBoxButton.OKCancel:
                case MessageBoxButton.OK:
                {
                    _Result = MessageBoxResult.OK;
                    break;
                }

                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                {
                    _Result = MessageBoxResult.Yes;
                    break;
                }
            }

            Kill_();
        }

        MessageBoxResult
            _Result = MessageBoxResult.Cancel;
        LocalDispatcherFrame
            _dispatcherFrame = new LocalDispatcherFrame(99786);
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
