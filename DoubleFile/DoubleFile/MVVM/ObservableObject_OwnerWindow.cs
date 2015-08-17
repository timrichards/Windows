using System.ComponentModel;

namespace DoubleFile
{
    abstract class Observable_OwnerWindowBase : ObservableObjectBase
    {
        static internal T DesignModeOK<T>(T retVal)
        {
            Util.Assert(99876, DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()));
            return retVal;
        }
    }
}
