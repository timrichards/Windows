﻿using System.IO;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    partial class MainWindow : LocalWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            GlobalData.GetInstance(this);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            ShowProjectWindow();
        }

        UList<LVitem_VolumeVM> m_list_lvVolStrings = null;

        void ShowProjectWindow(bool bOpenProject = false)
        {
            var volumes = new WinProject(m_list_lvVolStrings, bOpenProject);

            if (false == (volumes.ShowDialog() ?? false))
            {
                return;
            }

            m_list_lvVolStrings = volumes.m_list_lvVolStrings;

            if (m_list_lvVolStrings != null)
            {
                new SaveListingsProcess(m_list_lvVolStrings);
            }
        }

        private void LocalWindow_Closed(object sender, System.EventArgs e)
        {
            if (Directory.Exists(ProjectFile.TempPath))
            {
                Directory.Delete(ProjectFile.TempPath, true);
            }

            if (Directory.Exists(ProjectFile.TempPath01))
            {
                Directory.Delete(ProjectFile.TempPath01, true);
            }
        }

        private void Button_ViewProject_Click(object sender, RoutedEventArgs e)
        {
            ShowProjectWindow();
        }

        private void Button_OpenProject_Click(object sender, RoutedEventArgs e)
        {
            ShowProjectWindow(bOpenProject: true);
        }

        private void Button_SaveProject_Click(object sender, RoutedEventArgs e)
        {
            if (m_list_lvVolStrings != null)
            {
                WinProjectVM.SaveProject(m_list_lvVolStrings);
            }
            else
            {
                MBox.ShowDialog("No project to save.", "Save Project");
            }
        }
    }
}
