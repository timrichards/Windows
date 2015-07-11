using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace DoubleFile
{
    public class DataGrid_PreventHorizScrollOnClickAction : TriggerAction<DataGrid>
    {
        public object SaveHorizPosition
        {
            get { return GetValue(SaveHorizPositionProperty); }
            set { SetValue(SaveHorizPositionProperty, value); }
        }

        public static readonly DependencyProperty SaveHorizPositionProperty =
            DependencyProperty.Register("SaveHorizPosition", typeof(object),
            typeof(DataGrid_PreventHorizScrollOnClickAction), new UIPropertyMetadata(""));

        public object RestoreHorizPosition
        {
            get { return GetValue(RestoreHorizPositionProperty); }
            set { SetValue(RestoreHorizPositionProperty, value); }
        }

        public static readonly DependencyProperty RestoreHorizPositionProperty =
            DependencyProperty.Register("RestoreHorizPosition", typeof(object),
            typeof(DataGrid_PreventHorizScrollOnClickAction), new UIPropertyMetadata(""));

        protected override void Invoke(object parameter)
        {
            var bSaveHorizPosition = SaveHorizPosition is DataGrid;

            var scrollViewer = GetVisualChild<ScrollViewer>((DataGrid)
                (bSaveHorizPosition ? SaveHorizPosition : RestoreHorizPosition));

            if (bSaveHorizPosition)
            {
                scrollViewer.Tag = (double?)scrollViewer.ContentHorizontalOffset;
                return;
            }

            var nHorizontalOffset = (double?)scrollViewer.Tag;

            if (null == nHorizontalOffset)
                return;

            scrollViewer.ScrollToHorizontalOffset(nHorizontalOffset.Value);
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
