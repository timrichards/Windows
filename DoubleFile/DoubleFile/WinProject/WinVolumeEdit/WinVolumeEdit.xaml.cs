namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeEdit.xaml
    /// </summary>
    partial class WinVolumeEdit
    {
        public WinVolumeEdit()
        {
            InitializeComponent();
        }

        internal override LVitem_ProjectVM LVitemVolumeTemp
        {
            get
            {
                return new LVitem_ProjectVM(formUC_VolumeEdit.LVitemVolumeTemp);
            }

            set
            {
                formUC_VolumeEdit.LVitemVolumeTemp = new LVitem_ProjectVM(value);
            }
        }
    }
}
