﻿using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace SearchDirLists
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly GlobalData gd = null;
        internal readonly VolumesTabVM VolumesTab = null;
        internal readonly BrowseTabVM BrowseTab = null;

        public MainWindow()
        {
            gd = GlobalData.GetInstance(this);
            gd.m_blinky = new Blinky(xaml_cbFindbox);
            InitializeComponent();
            VolumesTab = new VolumesTabVM(this);
            BrowseTab = new BrowseTabVM(this);
        }
    }
}
