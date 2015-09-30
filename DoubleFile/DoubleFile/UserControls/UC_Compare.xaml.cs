namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_Compare
    {
        public UC_Compare()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _vm =
                new UC_CompareVM();
        }

        protected override void LocalNavigatedFrom()
        {
            DataContext =
                _vm =
                null;
        }

        UC_CompareVM
            _vm = null;
    }
}
