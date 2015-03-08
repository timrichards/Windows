namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormAnalysis_DirList.xaml
    /// </summary>
    public partial class WinFormAnalysis_DirList
    {
        internal WinFormAnalysis_DirList(MainWindow mainWindow, LV_ProjectVM LVprojectVM)
        {
            _mainWindow = mainWindow;
            _LVprojectVM = LVprojectVM;

            InitializeComponent();
            _form.Init(mainWindow, LVprojectVM);
            Activated += _form.ClearToolTip;
            Deactivated += _form.ClearToolTip;
            Closing += _form.FormAnalysis_DirList_FormClosing;
        }

        internal static void RestartTreeTimer(WinFormAnalysis_DirList arg1, LV_ProjectVM arg2)
        {
            FormAnalysis_DirList.RestartTreeTimer(arg1._form, arg2);
        }

        MainWindow _mainWindow = null;
        LV_ProjectVM _LVprojectVM = null;
    }
}
