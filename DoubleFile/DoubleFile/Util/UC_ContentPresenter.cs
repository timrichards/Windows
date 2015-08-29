using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_ContentPresenter.xaml
    /// </summary>
    public partial class UC_ContentPresenter : UserControl
    {
        public bool AllowFade { private get; set; } = true;

        public static readonly RoutedEvent
            ShownEvent = EventManager.RegisterRoutedEvent("Shown", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UC_ContentPresenter));
        public static readonly RoutedEvent
            HiddenEvent = EventManager.RegisterRoutedEvent("Hidden", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(UC_ContentPresenter));

        public event RoutedEventHandler Shown
        {
            add { AddHandler(ShownEvent, value); }
            remove { RemoveHandler(ShownEvent, value); }
        }

        public event RoutedEventHandler Hidden
        {
            add { AddHandler(HiddenEvent, value); }
            remove { RemoveHandler(HiddenEvent, value); }
        }

        void MakeFadeTrigger(Grid grid, RoutedEvent evt, double fromValue, double toValue)
        {
            var animation = new DoubleAnimation(fromValue, toValue, new Duration(TimeSpan.FromMilliseconds(200)));

            Storyboard.SetTargetProperty(animation, new PropertyPath("(Grid.Opacity)"));

            var storyboard = new Storyboard();

            storyboard.Children.Add(animation);

            var trigger = new EventTrigger(evt);

            trigger.Actions.Add(new BeginStoryboard { Storyboard = storyboard });
            grid.Triggers.Add(trigger);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            _grid.Children.Add(new ContentPresenter { Content = Content });
            Content = _grid;

            if (false == AllowFade)
                return;

            MakeFadeTrigger(_grid, ShownEvent, 0, 1);
            MakeFadeTrigger(_grid, HiddenEvent, 1, 0);
            _grid.Opacity = 0;
            Visibility = Visibility.Collapsed;
        }

        internal void LocalShow() { Visibility = Visibility.Visible; _grid.RaiseEvent(new RoutedEventArgs(ShownEvent)); }
        internal void LocalHide() { _grid.RaiseEvent(new RoutedEventArgs(HiddenEvent)); Visibility = Visibility.Collapsed; }

        Grid _grid = new Grid();
    }
}
