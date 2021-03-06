﻿namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_ClonesHere
    {
        public UC_ClonesHere()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _vm =
                new UC_ClonesHereVM { UseNicknames = _bNicknames, AllOneVolIsSolitary = _bAllOneVolIsSolitary }
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = _vm.UseNicknames;
            _bAllOneVolIsSolitary = _vm.AllOneVolIsSolitary;
            _vm?.Dispose();

            DataContext =
                _vm =
                null;
        }

        UC_ClonesHereVM
            _vm = null;
        bool
            _bNicknames = false;
        bool
            _bAllOneVolIsSolitary = false;
    }
}
