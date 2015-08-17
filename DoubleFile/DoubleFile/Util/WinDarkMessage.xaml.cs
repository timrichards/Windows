
using FirstFloor.ModernUI.Windows.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDarkMessage.xaml
    /// </summary>

    class WinDarkMessageVM : ObservableObjectBase
    {
        public ICommand Icmd_OK { get; private set; }
        public ICommand Icmd_Cancel { get; private set; }
        public string MessageboxText { get; private set; }
        public Visibility MessageboxVisibility { get; private set; }

        internal WinDarkMessageVM(WinDarkMessage winDarkMessage)
        {
            _winDarkMessage = winDarkMessage;
            Icmd_OK = new RelayCommand(Messagebox_OK);
            Icmd_Cancel = new RelayCommand(Messagebox_Cancel);
        }

        internal MessageBoxResult
            ShowMessagebox(string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
        {
            Util.UIthread(0, () =>
            {
                //Activate();

                Application.Current.Windows.OfType<ModernWindow>()
                        .ForEach(w => WinDarkMessage.SetDarkened(w, Visibility.Visible));

                MessageboxVisibility = Visibility.Visible;
                RaisePropertyChanged("MessageboxVisibility");

                MessageboxText = strMessage;
                RaisePropertyChanged("MessageboxText");
            });

            _dispatcherFrame_MessageBox.PushFrameTrue();
            return _messageboxResult;
        }
        LocalDispatcherFrame _dispatcherFrame_MessageBox = new LocalDispatcherFrame(0);
        MessageBoxResult _messageboxResult = MessageBoxResult.None;

        void Messagebox_Close()
        {
            Util.UIthread(0, () =>
            {
                Application.Current.Windows.OfType<ModernWindow>()
                    .ForEach(w => WinDarkMessage.SetDarkened(w, Visibility.Collapsed));

                MessageboxVisibility = Visibility.Collapsed;
                RaisePropertyChanged("MessageboxVisibility");
            });

            _dispatcherFrame_MessageBox.Continue = false;
        }

        void Messagebox_OK()
        {
            _messageboxResult = MessageBoxResult.OK;
            Messagebox_Close();
        }

        void Messagebox_Cancel()
        {
            _messageboxResult = MessageBoxResult.Cancel;
            Messagebox_Close();
        }

        readonly WinDarkMessage
            _winDarkMessage = null;
    }

    public partial class WinDarkMessage : UserControl
    {
        // "Darkened" dependency property
        public static Visibility GetDarkened(DependencyObject obj) => (Visibility)obj.GetValue(DarkenedProperty);
        public static void SetDarkened(DependencyObject obj, Visibility value) => obj.SetValue(DarkenedProperty, value);
        public static readonly DependencyProperty DarkenedProperty = DependencyProperty.RegisterAttached(
            "Darkened", typeof(Visibility), typeof(WinDarkMessage), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public WinDarkMessage()
        {
            DataContext =
                _winDarkMessageVM =
                new WinDarkMessageVM(this);

            InitializeComponent();
        }

        internal MessageBoxResult
            ShowMessagebox(string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
        {
            var retVal = _winDarkMessageVM.ShowMessagebox(strMessage, strTitle, buttons);

            return retVal;
        }

        WinDarkMessageVM
            _winDarkMessageVM = null;
    }
}

