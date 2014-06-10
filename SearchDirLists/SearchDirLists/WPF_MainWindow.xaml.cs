using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;

namespace SearchDirLists
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
#if (WPF)
            GlobalData.static_form = this;
#endif
            InitializeComponent();
        }

        // useful code but not implemented or working
        // https://stackoverflow.com/questions/4769916/how-to-get-a-listview-from-a-listviewitem

        readonly ListView listview = new ListView();
        private void Window_Loaded(object s, RoutedEventArgs args)
        {
            var collectionview = CollectionViewSource.GetDefaultView(this.listview.Items);
            collectionview.CollectionChanged += (sender, e) =>
            {
                if (e.NewItems != null && e.NewItems.Count > 0)
                {
                    var added = e.NewItems[0];
                    ListViewItem item = added as ListViewItem;
                    ListView parent = FindParent<ListView>(item);
                }
            };
        }

        public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = LogicalTreeHelper.GetParent(element) as FrameworkElement;

            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                    return correctlyTyped;
                else
                    return FindParent<T>(parent);
            }

            return null;
        }
    }
}
