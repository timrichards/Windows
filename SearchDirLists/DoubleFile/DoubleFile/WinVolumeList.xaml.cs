﻿using System.Windows;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeList.xaml
    /// </summary>
    partial class WinVolumeList : Window
    {
        public WinVolumeList()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = new LV_VolumeVM();
            var win = new WinVolumeListVM();

            form_lvVolumeList.DataContext = lv;
            form_WinVolumeList.DataContext = win;
            lv.SetPartner(win);
            win.SetPartner(lv);

            lv.SelectedOne = () => { return form_lvVolumeList.SelectedItems.Count == 1; };
            lv.SelectedAny = () => { return form_lvVolumeList.SelectedItems.Count > 0; };
            lv.Refresh = () => { }; // form_lvVolumeList.Items.Refresh(); });
            lv.Selected = () => { return form_lvVolumeList.SelectedItems.Cast<LVitem_VolumeVM>(); };

            lv.GetWindow = () => { return this; };
            win.GetWindow = () => { return this; };
        }

        internal UList<LVitem_VolumeVM> list_lvVolStrings = new UList<LVitem_VolumeVM>();

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            foreach (var lvItem in (form_lvVolumeList.DataContext as LV_VolumeVM).ItemsCast)
            {
                list_lvVolStrings.Add(lvItem);
            }

            // IsDefault = "True" in xaml so that seems to take care of closing the window After this handler returns.
        }
    }
}
