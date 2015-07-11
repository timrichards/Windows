using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace DoubleFile
{
    public class DataGrid_ScrollIntoViewAction : TriggerAction<DataGrid>
    {
        public object SelectionChanged
        {
            get { return GetValue(SelectionChangedProperty); }
            set { SetValue(SelectionChangedProperty, value); }
        }

        public static readonly DependencyProperty SelectionChangedProperty =
            DependencyProperty.Register("SelectionChanged", typeof(object),
            typeof(DataGrid_ScrollIntoViewAction), new UIPropertyMetadata(""));

        protected override void Invoke(object parameter)
        {
            DataGrid dataGrid = (DataGrid)SelectionChanged;

            if (null == dataGrid.SelectedItem)
                return;

            Util.UIthread(99820, () =>
            {
                dataGrid.UpdateLayout();
                dataGrid.ScrollIntoView(dataGrid.SelectedItem, null);
            });
        }
    }
}
