using System.Windows;

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
