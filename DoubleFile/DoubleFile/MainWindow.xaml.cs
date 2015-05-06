using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for ModernWindow1.xaml
    /// </summary>
    public partial class ModernWindow1
    {
        static Action Init = null;
        static void InitForMainWindowOnly(Action init) { Init = init; }
        public
            ModernWindow1()
            : base(InitForMainWindowOnly)
        {
            App.Icon = Icon;
            App.LocalMainWindow = this;
            Init();
            Init = null;
            InitializeComponent();
        }
    }
}
