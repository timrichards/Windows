using System.Reactive.Linq;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolders.xaml
    /// </summary>
    partial class WinTreeView
    {
        public WinTreeView()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            TreeViewVM.FactoryCreate(form_tv);
        }

        protected override void LocalNavigatedFrom()
        {
            form_tv.DataContext = null;
        }
    }
}
