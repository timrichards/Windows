using System.Reactive.Linq;
using System.Windows;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Files.xaml
    /// </summary>
    partial class WinDoubleFile_Files
    {
        public WinDoubleFile_Files()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => DataContext = _lvDoubleFile_FilesVM = new LV_DoubleFile_FilesVM());

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => _lvDoubleFile_FilesVM.Dispose());

            ResizeMode = ResizeMode.CanResize;
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Duplicates();
        }

        LV_DoubleFile_FilesVM
            _lvDoubleFile_FilesVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
