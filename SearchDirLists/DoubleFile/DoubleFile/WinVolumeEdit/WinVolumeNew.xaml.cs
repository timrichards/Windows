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
                return new LVitem_ProjectVM(uc_VolumeEdit.LVitemVolumeTemp);
            }

            set
            {
                uc_VolumeEdit.LVitemVolumeTemp = new LVitem_ProjectVM(value);
            }
        }
    }
}
