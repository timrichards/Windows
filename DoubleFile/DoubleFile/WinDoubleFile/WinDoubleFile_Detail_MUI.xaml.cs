namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Detail.xaml
    /// </summary>
    partial class WinDoubleFile_Detail_MUI
    {
        protected override void LocalNavigatedTo()
        {
            if (null != _lvFileDetailVM)
                _lvFileDetailVM.Dispose();

            if (null != _lvFolderDetailVM)
                _lvFolderDetailVM.Dispose();

            if (null != _lvVolumeDetailVM)
                _lvVolumeDetailVM.Dispose();

            form_localPath.DataContext =
                formLV_File.DataContext =
                _lvFileDetailVM =
                new LV_FileDetailVM();         
 
            formLV_Folder.DataContext =
                _lvFolderDetailVM =
                new LV_FolderDetailVM();

            formLV_Volume.DataContext =
                _lvVolumeDetailVM =
                new LV_VolumeDetailVM();
        }

        public WinDoubleFile_Detail_MUI()
        {
            InitializeComponent();
        }

        LV_FileDetailVM
            _lvFileDetailVM = null;
        LV_FolderDetailVM
            _lvFolderDetailVM = null;
        LV_VolumeDetailVM
            _lvVolumeDetailVM = null;
    }
}
