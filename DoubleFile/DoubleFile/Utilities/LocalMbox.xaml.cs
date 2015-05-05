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
    public partial class LocalMbox
    {
        internal string Message
        {
            get { return form_textBlock_Message.Text; }
            private set { form_textBlock_Message.Text = value; }
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
                        MBoxStatic.Assert(99943, false);
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
                MBoxStatic.Assert(99990, false);
            }
        }

        internal LocalMbox(string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => FlashWindowStatic.Go(this, Once: true));

            Observable.FromEventPattern(form_btnOK, "Click")
                .Subscribe(args => BtnOK_Click());

            Observable.FromEventPattern(form_btnCancel, "Click")
                .Subscribe(args => CloseIfSimulatingModal());

            Message = strMessage;

            if (null != strTitle)
                Title = strTitle;

            if (null != buttons)
                Buttons = buttons.Value;
        }

        internal LocalMbox(ILocalWindow owner, string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
            : this(strMessage, strTitle, buttons)
        {
            Owner = owner as Window;
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

            base.ShowDialog();
            return _Result;
        }

        private void BtnOK_Click()
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
