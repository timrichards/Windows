using System.Windows;

namespace DoubleFile
{
    public class BindingProxy : Freezable
    {
        protected override Freezable
            CreateInstanceCore() => new BindingProxy();

        public object Value
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty
            DataProperty = DependencyProperty.Register("Value", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
