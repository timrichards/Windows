using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleFile
{
    class ObservableObject_OwnerWindow : ObservableObjectBase
    {
        internal delegate System.Windows.Window Delegate_GetWindow();
        internal Delegate_GetWindow GetWindow = () => { DesignModeOK(); return null; };

        static internal void DesignModeOK()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()) == false)
            {
                MBox.Assert(0, false);
            }
        }
    }
}
