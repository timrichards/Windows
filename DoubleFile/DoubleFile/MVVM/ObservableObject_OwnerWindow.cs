using System.ComponentModel;

namespace DoubleFile
{
    abstract class Observable_OwnerWindowBase : ObservableObjectBase
    {
        static internal T DesignModeOK<T>(T retVal)
        {
            if (false ==
                DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                Util.Assert(99876, false);
            }

            return retVal;
        }
    }
}
