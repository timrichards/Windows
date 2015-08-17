using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for LocalContentPresenter.xaml
    /// </summary>
    public partial class LocalContentPresenter : UserControl
    {
        //[Category("LocalContentPresenter")]
        //public object Presenter
        //{
        //    get { return GetValue(PresenterProperty); }
        //    set { SetValue(PresenterProperty, value); }
        //}

        //public static readonly DependencyProperty
        //    PresenterProperty = DependencyProperty.Register("Presenter", typeof(object), typeof(LocalContentPresenter),
        //    new FrameworkPropertyMetadata(null));

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var grid = new Grid();

            //grid.Children.Add(
            //    new ContentPresenter { Content = BindingOperations.SetBinding(this, PresenterProperty, new Binding()) });

            grid.Children.Add(
                new ContentPresenter { Content = Content });

            Content = grid;
        }
    }
}
