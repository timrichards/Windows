using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DoubleFile
{
    public partial class UC_Messagebox : UC_ContentPresenter
    {
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
            _OK_ColumnProperty = formBtn_OK.GetValue(Grid.ColumnProperty);
        }

        internal MessageBoxResult
            ShowMessagebox(string strMessage, string strTitle, MessageBoxButton? buttons)
        {
            Observable.FromEventPattern(formBtn_OK, "Click")
                .LocalSubscribe(99754, x => BtnOK_Click());

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .LocalSubscribe(99753, x => _dispatcherFrame.Continue = false);

            //if (null != strTitle)
            //    Title = strTitle;

            if (null != buttons)
                Buttons = buttons.Value;

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

            form_Message.Text = strMessage;
            LocalShow();
            _dispatcherFrame.PushFrameTrue();
            LocalHide();
            return _Result;
        }

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

            _dispatcherFrame.Continue = false;
        }

        MessageBoxResult _Result = MessageBoxResult.Cancel;
        LocalDispatcherFrame _dispatcherFrame = new LocalDispatcherFrame(99786);
    }
}
