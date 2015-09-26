using System.Collections.Generic;
using System.Windows;
using System.Windows.Interactivity;

namespace DoubleFile
{
    public class Behaviors : List<Behavior>
    {
    }

    public class Triggers : List<System.Windows.Interactivity.TriggerBase>
    {
    }

    public static class SupplementaryInteraction
    {
        public static Behaviors
            GetBehaviors(DependencyObject obj) => (Behaviors)obj.GetValue(BehaviorsProperty);

        public static void
            SetBehaviors(DependencyObject obj, Behaviors value) => obj.SetValue(BehaviorsProperty, value);

        public static readonly DependencyProperty
            BehaviorsProperty = DependencyProperty.RegisterAttached("Behaviors", typeof(Behaviors),
            typeof(SupplementaryInteraction), new UIPropertyMetadata(null, OnPropertyBehaviorsChanged));

        static void
            OnPropertyBehaviorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(d);

            ((Behaviors)e.NewValue)
                .ForEach(behaviors.Add);
        }

        public static Triggers
            GetTriggers(DependencyObject obj) => (Triggers)obj.GetValue(TriggersProperty);

        public static void
            SetTriggers(DependencyObject obj, Triggers value) => obj.SetValue(TriggersProperty, value);

        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.RegisterAttached("Triggers", typeof(Triggers),
            typeof(SupplementaryInteraction), new UIPropertyMetadata(null, OnPropertyTriggersChanged));

        static void OnPropertyTriggersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var triggers = Interaction.GetTriggers(d);

            ((Triggers)e.NewValue)
                .ForEach(triggers.Add);
        }
    }
}
