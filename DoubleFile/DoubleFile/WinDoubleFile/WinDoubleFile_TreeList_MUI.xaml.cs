namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinDoubleFile_TreeList_MUI
    {
        protected override void LocalNavigatedTo()
        {
            WinProject_MUI.InitExplorer();
        }

        public WinDoubleFile_TreeList_MUI()
        {
            InitializeComponent();

            var lvChildrenVM = new LV_TreeListChildrenVM();

            form_lvChildren.DataContext = lvChildrenVM;
            form_lvSiblings.DataContext = new LV_TreeListSiblingsVM(lvChildrenVM);
        }
    }
}
