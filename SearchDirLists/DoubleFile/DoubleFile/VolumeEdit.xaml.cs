using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for VolumeEdit.xaml
    /// </summary>
    public partial class VolumeEdit : Window
    {
        public string[] StringValues
        {
            get
            {
                return uc_VolumeEdit.StringValues;
            }

            set
            {
                uc_VolumeEdit.StringValues = value;
            }
        }

        public VolumeEdit()
        {
            InitializeComponent();
        }
    }
}
