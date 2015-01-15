namespace DoubleFile
{
    public class LocalWindow : System.Windows.Window
    {
        public LocalWindow()
        {
            this.Activated += new System.EventHandler((o, e) => { GlobalData.static_TopWindow = this; });
        }

        public new void Show() { MBox.Assert(0, false, "Pass in parent window."); }
        public new void ShowDialog() { MBox.Assert(0, false, "Pass in parent window."); }

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
