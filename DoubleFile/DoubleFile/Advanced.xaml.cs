using System.Reactive.Linq;
using System.Windows.Controls;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for Advanced.xaml
    /// </summary>
    public partial class Advanced : UserControl
    {
        public Advanced()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_btnNewWindow, "Click")
                .Subscribe(args => { new ModernWindow1().Show(); });
        }
    }
}
