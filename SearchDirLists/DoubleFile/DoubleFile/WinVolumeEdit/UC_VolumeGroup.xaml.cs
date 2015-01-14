using System.Windows.Controls;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_VolumeGroup.xaml
    /// </summary>
    public partial class UC_VolumeGroup : UserControl
    {
        public UC_VolumeGroup()
        {
            InitializeComponent();
        }

        public string Text { get { return form_EditVolumeGroup.Text; } set { form_EditVolumeGroup.Text = value; } }
    }
}
