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
            var grid = sender.As<DataGrid>();

            if ((null == grid) ||
                (null == grid.SelectedItem))
            {
                return;
            }

            Util.UIthread(99817, () =>
            {
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.SelectedItem, null);
            });
        }
    }
}
