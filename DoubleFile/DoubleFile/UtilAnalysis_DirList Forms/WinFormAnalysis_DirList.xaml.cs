namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormAnalysis_DirList.xaml
    /// </summary>
    public partial class WinFormAnalysis_DirList
    {
        internal WinFormAnalysis_DirList(MainWindow mainWindow, LV_ProjectVM LVprojectVM)
        {
            InitializeComponent();
            _form.Init(mainWindow, LVprojectVM);
            Activated += _form.ClearToolTip;
            Deactivated += _form.ClearToolTip;
            Closing += _form.HostClosing;
            Closed += (o,e) => _host.Dispose();
            Loaded += _form.FormAnalysis_DirList_Load;
        }

        internal static void RestartTreeTimer(WinFormAnalysis_DirList form1, LV_ProjectVM lvProjectVM)
        {
            FormAnalysis_DirList.RestartTreeTimer(form1._form, lvProjectVM);
        }
    }
}
