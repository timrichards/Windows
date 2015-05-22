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
            new TreeViewVM(form_tv, LocalTV.RootNodes);
        }

        protected override void LocalWindowClosed()
        {
            form_tv.DataContext = null;
        }
    }
}
