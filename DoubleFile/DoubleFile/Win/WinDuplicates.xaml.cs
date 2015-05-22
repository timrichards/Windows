using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDuplicates.xaml
    /// </summary>
    partial class WinDuplicates
    {
        public WinDuplicates()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _winDuplicatesVM =
                new WinDuplicatesVM();
        }

        protected override void LocalNavigatedFrom()
        {
            if (null != _winDuplicatesVM)
                _winDuplicatesVM.Dispose();
        }

        WinDuplicatesVM
            _winDuplicatesVM = null;
    }
}
