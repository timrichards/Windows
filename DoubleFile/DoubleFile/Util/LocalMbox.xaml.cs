using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for LocalMbox.xaml
    /// </summary>
    public partial class LocalMbox : IModalWindow
    {
        internal string Message
        {
            get { return formTextBlock_Message.Text; }
            private set { formTextBlock_Message.Text = value; }
        }

        MessageBoxButton Buttons
        {
            set
            {
                _buttons = value;

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

        public LocalMbox()
        {
            if (false ==
                DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                Util.Assert(99990, false);
            }
        }

        internal LocalMbox(ILocalWindow owner, string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
        {
            try
            {
                InitializeComponent();
            }
            catch
            {
                MessageBox.Show("Could not initialize component while asserting another error (next...)");
                MessageBox.Show(strMessage, strTitle, buttons ?? MessageBoxButton.OK);
                return;
            }

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;

            var bMainWindowLoaded = Application.Current?.MainWindow?.IsLoaded ?? false;

            var winOwner =
                (false == (owner?.LocalIsClosed ?? true))
                ? (Window)owner
                : bMainWindowLoaded
                ? Application.Current.MainWindow
                : null;

            Owner =
                winOwner?.IsLoaded ?? false
                ? winOwner
                : null;

            // use-case: assert before main window shown
            var rc =
                (null != Owner)
                ? Win32Screen.GetWindowRect(Owner)
                : Win32Screen.GetWindowMonitorInfo(this).rcWork;

            Left = rc.Left;
            Width = rc.Width;

            Observable.FromEventPattern(this, "SourceInitialized")
                .LocalSubscribe(99756, x => ResizeMode = ResizeMode.NoResize);

            Observable.FromEventPattern(this, "ContentRendered")
                .LocalSubscribe(99755, x => Win32Screen.FlashWindow(this, Once: true));

            Observable.FromEventPattern(formBtn_OK, "Click")
                .LocalSubscribe(99754, x => BtnOK_Click());

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .LocalSubscribe(99753, x => CloseIfSimulatingModal());

            Message = strMessage;

            if (null != strTitle)
                Title = strTitle;

            if (null != buttons)
                Buttons = buttons.Value;
        }

        internal new MessageBoxResult ShowDialog()
        {
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

            // use-case: assert before main window shown
            if (false != (Application.Current.MainWindow?.IsLoaded ?? false))
                base.ShowDialog();
            else
                ((Window)this).ShowDialog();

            if (false == IsLoaded)
            {
                MessageBox.Show("The local message box could not open to display the following message (next...)");
                _Result = MessageBox.Show(Message, Title, _buttons);
            }

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

            LocalDialogResult = true;

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
            // Not when simulating modal.
            CloseIfSimulatingModal();
        }

        MessageBoxResult _Result = MessageBoxResult.Cancel;
    }
}
