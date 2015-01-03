using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoubleFile
{
    class ObservableObject_OwnerWindow : ObservableObject
    {
        internal delegate System.Windows.Window Delegate_GetWindow();
        internal Delegate_GetWindow GetWindow = () => { System.Diagnostics.Debug.Assert(false); return null; };

        static internal void DesignModeOK()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()) == false)
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
    }
}
