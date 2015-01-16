namespace DoubleFile
{
    public class LocalWindow : System.Windows.Window
    {
        public LocalWindow()
        {
            this.Activated += new System.EventHandler((o, e) => { GlobalData.static_TopWindow = this; });
        }

        internal new void Show() { MBox.Assert(0, false, "Pass in parent window."); }
        internal new bool? ShowDialog() { MBox.Assert(0, false, "Pass in parent window."); return null; }

        internal void Show(LocalWindow me)
        {
            Owner = me;
            base.Show();
        }

        internal bool? ShowDialog(LocalWindow me)
        {
            Owner = me;
            return base.ShowDialog();
        }
    }
}
