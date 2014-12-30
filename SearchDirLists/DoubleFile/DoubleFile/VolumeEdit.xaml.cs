using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for VolumeEdit.xaml
    /// </summary>
    public partial class VolumeEdit : Window
    {
        public VolumeEdit()
        {
            InitializeComponent();
        }

        private void form_VolumeEdit_Initialized(object sender, EventArgs e)
        {
            new VolumeEditVM(this);
        }
    }
}
