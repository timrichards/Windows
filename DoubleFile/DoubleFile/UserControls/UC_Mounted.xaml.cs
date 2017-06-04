namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_Mounted
    {
        public UC_Mounted()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _vm =
                new UC_MountedVM { UseNicknames = _bNicknames }
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

        UC_MountedVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
