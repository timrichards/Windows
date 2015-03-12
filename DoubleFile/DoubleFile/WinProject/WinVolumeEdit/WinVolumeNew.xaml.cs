namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeNew.xaml
    /// </summary>
    partial class WinVolumeNew : WinVolumeEditBase
    {
        public WinVolumeNew()
        {
            InitializeComponent();
        }

        internal override LVitem_ProjectVM LVitemVolumeTemp
        {
            get
            {
                return new LVitem_ProjectVM(form_ucVolumeEdit.LVitemVolumeTemp);
            }

            set
            {
                form_ucVolumeEdit.LVitemVolumeTemp = new LVitem_ProjectVM(value);
            }
        }
    }
}
