namespace DoubleFile
{
    public abstract class LocalWindowBase : System.Windows.Window
    {
        public LocalWindowBase()
        {
            this.Activated += new System.EventHandler((o, e) => { GlobalData.static_TopWindow = this; });
        }
    }
}
