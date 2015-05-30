﻿using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for LocalMbox.xaml
    /// </summary>
    public partial class LocalMbox
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

            var rc = MainWindow.WithMainWindow(Win32Screen.GetWindowRect);

            Left = rc.Left;
            Width = rc.Width;

            Observable.FromEventPattern(this, "SourceInitialized")
                .Subscribe(x => ResizeMode = ResizeMode.NoResize);

            Observable.FromEventPattern(this, "ContentRendered")
                .Subscribe(x => Win32Screen.FlashWindow(this, Once: true));

            Observable.FromEventPattern(formBtn_OK, "Click")
                .Subscribe(x => BtnOK_Click());

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .Subscribe(x => CloseIfSimulatingModal());

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