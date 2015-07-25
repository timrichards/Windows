using System;

namespace VolTreeMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        static Action Init = null;
        static void InitForMainWindowOnly(Action init) { Init = init; }
        public
            MainWindow()
            : base(InitForMainWindowOnly)
        {
     //       Statics.Icon = Icon;
            Init();
            Init = null;
            InitializeComponent();
        }
    }
}
