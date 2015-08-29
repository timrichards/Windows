using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_ContentPresenter.xaml
    /// </summary>
    public partial class UC_ContentPresenter : UserControl
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var grid = new Grid();

            grid.Children.Add(new ContentPresenter { Content = Content });
            Content = grid;
        }
    }
}
