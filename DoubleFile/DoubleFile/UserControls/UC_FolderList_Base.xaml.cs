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

            var sidePanelsCtls = new UIElementCollection(this, this);

            SetValue(SidePanelCtlsProperty, sidePanelsCtls);

            Observable.FromEventPattern(this, "Loaded")
                .LocalSubscribe(99617, x =>
            {
                foreach (UIElement element in sidePanelsCtls.Cast<UIElement>().ToList())
                {
                    sidePanelsCtls.Remove(element);
                    form_StackPanel.Children.Add(element);
                }

                var margin = new Thickness(10, 0, 0, 0);

                foreach (var child in form_StackPanel.Children.OfType<FrameworkElement>())
                    child.Margin = margin;
            });
        }

        public UIElementCollection
            SidePanelCtls => (UIElementCollection)GetValue(SidePanelCtlsProperty);
        public static readonly DependencyProperty
            SidePanelCtlsProperty = DependencyProperty.Register("SidePanelCtls", typeof(UIElementCollection), typeof(UC_FolderList_Base));
     }
}
