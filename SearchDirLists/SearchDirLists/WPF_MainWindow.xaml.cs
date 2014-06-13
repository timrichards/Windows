using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace SearchDirLists
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal readonly GlobalData gd = null;

        public MainWindow()
        {
            gd = GlobalData.GetInstance(this);
            gd.m_blinky = new Blinky(xaml_cbFindbox);
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

        private void xaml_cbSaveAs_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (new Key[] {Key.Enter, Key.Return}.Contains(e.Key))
            {
#if (WPF)
                xaml_btnAddVolume.Command.Execute(null);
#endif
            }
        }
    }
}
