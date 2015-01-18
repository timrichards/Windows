namespace DoubleFile
{
    class ObservableObject_OwnerWindow : ObservableObjectBase
    {
        static internal void DesignModeOK()
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()) == false)
            {
                MBox.Assert(0, false);
            }
        }
    }
}
