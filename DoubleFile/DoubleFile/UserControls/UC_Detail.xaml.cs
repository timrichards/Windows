using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDetail.xaml
    /// </summary>
    partial class UC_Detail
    {
        public UC_Detail()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
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

            foreach (var ctl in new[] { formLV_File, formLV_Folder, formLV_Volume })
            {
                Observable.FromEventPattern(ctl, "SelectionChanged")
                    .LocalSubscribe(99686, x => ctl.UnselectAllCells());    // in C# 4 closure on ctl == formLV_Volume 3x
            }
        }

        protected override void LocalNavigatedFrom()
        {
            _lvFileDetailVM?.Dispose();
            _lvFolderDetailVM?.Dispose();
            _lvVolumeDetailVM?.Dispose();

            form_localPath.DataContext =
                formLV_File.DataContext =
                _lvFileDetailVM =
                null;

            formLV_Folder.DataContext =
                _lvFolderDetailVM =
                null;

            formLV_Volume.DataContext =
                _lvVolumeDetailVM =
                null;
        }

        LV_FileDetailVM
            _lvFileDetailVM = null;
        LV_FolderDetailVM
            _lvFolderDetailVM = null;
        LV_VolumeDetailVM
            _lvVolumeDetailVM = null;
    }
}
