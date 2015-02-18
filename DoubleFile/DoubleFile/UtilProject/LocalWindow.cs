namespace DoubleFile
{
    public class LocalWindow : System.Windows.Window
    {
        internal bool IsClosed = true;

        internal new void Show() { Init(); Show(GlobalData.static_Dialog); }
        internal new bool? ShowDialog() { Init(); return ShowDialog(GlobalData.static_Dialog); }

        protected void Init()
        {
            Loaded += (o, e) => IsClosed = false;
            Closed += (o, e) => IsClosed = true;
        }

        void Show(LocalWindow me)
        {
            Owner = me;
            base.Show();
        }

        bool? ShowDialog(LocalWindow me)
        {
            GlobalData.static_Dialog = this;
            Owner = me;

            Closed += (o, e) =>
            {
                GlobalData.static_Dialog = me;
                me.Activate();
            };

            return base.ShowDialog();
        }
    }
}
