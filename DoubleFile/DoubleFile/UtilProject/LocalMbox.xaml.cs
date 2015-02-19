using System.Windows;
using System.Windows.Controls;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for LocalMbox.xaml
    /// </summary>
    public partial class LocalMbox
    {
        internal string Message
        {
            get { return Message_.Text; }
            private set { Message_.Text = value; }
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
                            form_btnCancel.Visibility = Visibility.Hidden;
                            form_btnOK.SetValue(Grid.ColumnProperty, form_btnCancel.GetValue(Grid.ColumnProperty));
                            break;
                        }

                    case MessageBoxButton.YesNo:
                        {
                            form_btnOK.Content = "Yes";
                            form_btnCancel.Content = "No";
                            break;
                        }

                    case MessageBoxButton.YesNoCancel:
                        {
                            MBoxStatic.Assert(0, false);
                            break;
                        }
                }
            }
        }
        MessageBoxButton _buttons = MessageBoxButton.OKCancel;

        internal LocalMbox(LocalWindow owner, string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
            : this(strMessage, strTitle, buttons)
        {
            Owner = owner;
        }

        internal LocalMbox(string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
        {
            InitializeComponent();
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
                        Result = MessageBoxResult.Cancel;
                        break;
                    }

                case MessageBoxButton.OK:
                    {
                        Result = MessageBoxResult.OK;
                        break;
                    }

                case MessageBoxButton.YesNo:
                    {
                        Result = MessageBoxResult.No;
                        break;
                    }
            }

            base.ShowDialog();
            return Result;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            switch (_buttons)
            {
                case MessageBoxButton.OKCancel:
                case MessageBoxButton.OK:
                    {
                        Result = MessageBoxResult.OK;
                        break;
                    }

                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                    {
                        Result = MessageBoxResult.Yes;
                        break;
                    }
            }

            LocalDialogResult = true;

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
            // Not when simulating modal.
            CloseIfSimulatingModal();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseIfSimulatingModal();
        }

        MessageBoxResult Result = MessageBoxResult.Cancel;
    }
}
