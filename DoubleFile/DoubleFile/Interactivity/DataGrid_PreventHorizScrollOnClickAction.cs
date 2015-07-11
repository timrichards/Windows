using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace DoubleFile
{
    public class DataGrid_PreventHorizScrollOnClickAction : TriggerAction<DataGrid>
    {
        public object PreviewMouseDown
        {
            get { return GetValue(PreviewMouseDownProperty); }
            set { SetValue(PreviewMouseDownProperty, value); }
        }

        public static readonly DependencyProperty PreviewMouseDownProperty =
            DependencyProperty.Register("PreviewMouseDown", typeof(object),
            typeof(DataGrid_PreventHorizScrollOnClickAction), new UIPropertyMetadata(""));

        public object SelectedCellsChanged
        {
            get { return GetValue(SelectedCellsChangedProperty); }
            set { SetValue(SelectedCellsChangedProperty, value); }
        }

        public static readonly DependencyProperty SelectedCellsChangedProperty =
            DependencyProperty.Register("SelectedCellsChanged", typeof(object),
            typeof(DataGrid_PreventHorizScrollOnClickAction), new UIPropertyMetadata(""));

        protected override void Invoke(object parameter)
        {
            var scrollVieweer = GetVisualChild<ScrollViewer>((DataGrid)
                (parameter is MouseButtonEventArgs ? PreviewMouseDown : SelectedCellsChanged));

            if (parameter is MouseButtonEventArgs)
                scrollVieweer.Tag = scrollVieweer.ContentHorizontalOffset;
            else
                scrollVieweer.ScrollToHorizontalOffset((double)scrollVieweer.Tag);
        }

        static T GetVisualChild<T>(DependencyObject parent) where T : Visual
        {
            var child = default(T);
            var numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++)
            {
                var v = (Visual)VisualTreeHelper.GetChild(parent, i);
                
                child = v.As<T>();

                if (null == child)
                    child = GetVisualChild<T>(v);

                if (null != child)
                    break;
            }

            return child;
        }
    }
}
