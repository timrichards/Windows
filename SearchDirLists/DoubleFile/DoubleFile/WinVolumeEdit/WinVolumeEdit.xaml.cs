using System.Windows;

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

        internal override LVitem_VolumeVM LVitemVolumeTemp
        {
            get
            {
                return new LVitem_VolumeVM(uc_VolumeEdit.LVitemVolumeTemp);
            }

            set
            {
                uc_VolumeEdit.LVitemVolumeTemp = new LVitem_VolumeVM(value);
            }
        }
    }
}
