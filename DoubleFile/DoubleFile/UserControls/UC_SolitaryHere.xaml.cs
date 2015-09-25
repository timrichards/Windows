namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_SolitaryHere
    {
        public UC_SolitaryHere()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext = 
                _vm =
                new UC_SolitaryHereVM { UseNicknames = _bNicknames, SolitaryIsAllOneVol = _bSolitaryIsAllOneVol }
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = _vm.UseNicknames;
            _bSolitaryIsAllOneVol = _vm.SolitaryIsAllOneVol;
            _vm?.Dispose();

            DataContext =
                _vm =
                null;
        }

        UC_SolitaryHereVM
            _vm = null;
        bool
            _bNicknames = false;
        bool
            _bSolitaryIsAllOneVol = false;
    }
}
