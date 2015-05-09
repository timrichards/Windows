﻿namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Search_MUI
    {
        protected override void LocalNavigatedTo()
        {
            WinProject_MUI.InitExplorer();
        }

        public WinDoubleFile_Search_MUI()
        {
            InitializeComponent();
            
            DataContext = new WinDoubleFile_SearchVM
            {
                IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(form_searchText.Text)
            }.Init();
        }
    }
}
