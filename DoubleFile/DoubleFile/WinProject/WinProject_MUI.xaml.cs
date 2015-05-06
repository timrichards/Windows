using System.Windows;
using System.Linq;
using System.Reactive.Linq;
using System;
using System.Collections.Generic;
using FirstFloor.ModernUI.Windows;
using FirstFloor.ModernUI.Windows.Navigation;
using System.Windows.Controls;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinProject.xaml
    /// </summary>
    partial class WinProject_MUI
    {
        public WinProject_MUI()
        {
            InitializeComponent();

            App.LVprojectVM = new LV_ProjectVM(App.LVprojectVM)
            {
                SelectedOne = () => form_lv.SelectedItems.HasOnlyOne(),
                SelectedAny = () => false == form_lv.SelectedItems.IsEmptyA(),
                Selected = () => form_lv.SelectedItems.Cast<LVitem_ProjectVM>()
            };

            var vm = new WinProjectVM(App.LVprojectVM);

            form_lv.DataContext = App.LVprojectVM;
            DataContext = vm;
        }

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
