namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFiles.xaml
    /// </summary>
    partial class UC_Files
    {
        public UC_Files()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _vm =
                new LV_FilesVM();
        }

        protected override void LocalNavigatedFrom()
        {
            _vm?.Dispose();

            DataContext =
                _vm =
                null;
        }

        LV_FilesVM
            _vm = null;
    }
}
