using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for VolumeNew.xaml
    /// </summary>
    public partial class VolumeNew : Window
    {
        public string[] StringValues
        {
            get
            {
                return uc_VolumeEdit.StringValues;
            }
        }

        public VolumeNew()
        {
            InitializeComponent();
        }
    }
}
