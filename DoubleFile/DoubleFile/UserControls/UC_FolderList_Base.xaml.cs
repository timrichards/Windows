using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace DoubleFile
{
    [ContentProperty("SidePanelCtls")]
    public partial class UC_FolderList_Base
    {
        public UC_FolderList_Base()
        {
            InitializeComponent();

            var margin = new Thickness(10, 0, 0, 0);

            Observable.FromEventPattern(form_StackPanel, "LayoutUpdated")
                .LocalSubscribe(99617, x =>
            {
                foreach (var child in form_StackPanel.Children.OfType<FrameworkElement>())
                    child.Margin = margin;
            });

            SidePanelCtls = new UIElementCollection(this, this);

            Observable.FromEventPattern(this, "Loaded")
                .LocalSubscribe(99616, x =>
            {
                foreach (UIElement element in SidePanelCtls.Cast<UIElement>().ToList())
                {
                    SidePanelCtls.Remove(element);
                    form_StackPanel.Children.Add(element);
                }
            });
        }

        public UIElementCollection SidePanelCtls
        {
            get { return (UIElementCollection)GetValue(SidePanelCtlsProperty); }
            set { SetValue(SidePanelCtlsProperty, value); }
        }

        public static readonly DependencyProperty
            SidePanelCtlsProperty = DependencyProperty.Register("SidePanelCtls", typeof(UIElementCollection), typeof(UC_FolderList_Base));
     }
}
