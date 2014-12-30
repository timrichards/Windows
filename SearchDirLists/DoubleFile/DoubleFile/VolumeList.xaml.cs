using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for VolumeList.xaml
    /// </summary>
    public partial class VolumeList : Window
    {
        public VolumeList()
        {
            InitializeComponent();
        }

        private void form_VolumeList_Initialized(object sender, System.EventArgs e)
        {
            new VolumeListVM(this);
        }
    }
}
