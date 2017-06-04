namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_Sinks
    {
        public UC_Sinks()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _vm =
                new UC_SinksVM { UseNicknames = _bNicknames }
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = _vm.UseNicknames;
            _vm?.Dispose();

            DataContext =
                _vm =
                null;
        }

        UC_SinksVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
