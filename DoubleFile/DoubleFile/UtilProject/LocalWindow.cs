namespace DoubleFile
{
    public class LocalWindow : System.Windows.Window
    {
        public LocalWindow()
        {
            this.Activated += new System.EventHandler((o, e) => { GlobalData.static_TopWindow = this; });
         //   this.Closed += new System.EventHandler((o, e) => { GlobalData.static_TopWindow.Activate(); });
        }

        internal new void Show() { Show(GlobalData.static_TopWindow); }
        internal new bool? ShowDialog() { return ShowDialog(GlobalData.static_TopWindow); }

        void Show(LocalWindow me)
        {
            Owner = me;
            base.Show();
        }

        bool? ShowDialog(LocalWindow me)
        {
            Owner = me;
            return base.ShowDialog();
        }
    }
}
