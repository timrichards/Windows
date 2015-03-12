namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinFormDirList
    {
        internal WinFormDirList(MainWindow mainWindow, LV_ProjectVM LVprojectVM)
        {
            InitializeComponent();
            _form.Init(mainWindow, LVprojectVM);
            Activated += _form.ClearToolTip;
            Deactivated += _form.ClearToolTip;
            Closing += _form.HostClosing;
            Closed += (o,e) => _host.Dispose();
            Loaded += _form.FormDirList_Load;
        }

        internal static void RestartTreeTimer(WinFormDirList form1, LV_ProjectVM lvProjectVM)
        {
            FormDirList.RestartTreeTimer(form1._form, lvProjectVM);
        }
    }
}
