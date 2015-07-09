using System.Windows.Controls;
using System.Windows.Interactivity;

namespace DoubleFile
{
    public class DataGrid_ScrollIntoViewBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (false == sender is DataGrid)
                return;

            var grid = (DataGrid)sender;

            if (null == grid.SelectedItem)
                return;

            Util.UIthread(() =>
            {
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.SelectedItem, null);
            });
        }
    }
}
