using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            GlobalData.GetInstance(this);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ShowVolumeList();
        }

        UList<LVitem_VolumeVM> m_list_lvVolStrings = null;

        void ShowVolumeList()
        {
            var m_volumes = new WinVolumeList(m_list_lvVolStrings);

            if (false == (m_volumes.ShowDialog(this) ?? false))
            {
                return;
            }

            m_list_lvVolStrings = m_volumes.m_list_lvVolStrings;

            if (m_list_lvVolStrings != null)
            {
                new SaveListingsProcess(m_list_lvVolStrings);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShowVolumeList();
        }
    }
}
