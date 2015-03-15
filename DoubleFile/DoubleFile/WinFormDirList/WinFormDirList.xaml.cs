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
            _form.form_tmapUserCtl.LocalOwner = this;
            Closing += _form.HostClosing;
            Closed += (o,e) => _host.Dispose();
            Loaded += _form.FormDirList_Load;
            LocationChanged += _form.ClearToolTip;
        }

        internal static void RestartTreeTimer(WinFormDirList form1, LV_ProjectVM lvProjectVM)
        {
            FormDirList.RestartTreeTimer(form1._form, lvProjectVM);
        }
    }
}
