namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeEdit.xaml
    /// </summary>
    partial class WinVolumeEdit : WinVolumeEditBase
    {
        public WinVolumeEdit()
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
