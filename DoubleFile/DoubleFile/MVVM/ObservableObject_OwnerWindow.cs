using System.ComponentModel;
namespace DoubleFile
{
    class ObservableObject_OwnerWindow : ObservableObjectBase
    {
        static internal void DesignModeOK()
        {
            if (false ==
                DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
            {
                MBoxStatic.Assert(0, false);
            }
        }
    }
}
