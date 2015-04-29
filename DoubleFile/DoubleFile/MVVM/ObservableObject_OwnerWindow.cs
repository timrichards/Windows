using System.ComponentModel;

namespace DoubleFile
{
    abstract class Observable_OwnerWindowBase : ObservableObjectBase
    {
        static internal void DesignModeOK()
        {
            if (false ==
                DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                MBoxStatic.Assert(99990, false);
            }
        }
    }
}
