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

            gd = GlobalData.GetInstance(this);
            m_app = this;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var volumes = new WinVolumeList();

            volumes.ShowDialog(this);

            var listNicknames = new List<string>();
            var listPaths = new List<string>();

            foreach (LVitem_VolumeVM volStrings in volumes.list_lvVolStrings)
            {
                if ((FileParse.mSTRusingFile + FileParse.mSTRsaved + FileParse.mSTRcantSave).Contains(volStrings.Status))
                {
                    continue;
                }

                listNicknames.Add(volStrings.VolumeName);
                listPaths.Add(volStrings.Path);
            }

            (m_winProgress = new WinSaveInProgress()).Show(this);
            m_winProgress.InitProgress(listNicknames, listPaths);
            (gd.m_saveDirListings = new SaveDirListings(volumes.list_lvVolStrings, SaveDirListingsStatusCallback, SaveDirListingsDoneCallback)).DoThreadFactory();
        }

        readonly GlobalData gd = null;
        readonly MainWindow m_app = null;
    }
}
