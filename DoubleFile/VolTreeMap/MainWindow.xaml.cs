using DoubleFile;
using FirstFloor.ModernUI.Presentation;
using System;

namespace VolTreeMap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        class MyLink : Link
        {
            internal MyLink(string strDisplayName, string strSource)
            {
                DisplayName = strDisplayName;
                Source = new Uri("/DoubleFile;component" + strSource, UriKind.Relative);
            }
        }

        static Action Init = null;
        static void InitForMainWindowOnly(Action init) { Init = init; }
        public
            MainWindow()
            : base(InitForMainWindowOnly)
        {
            Statics.Icon = Icon;
            Init();
            Init = null;
            InitializeComponent();

            MenuLinkGroups.Add(new LinkGroup { DisplayName="Explore", Links =
            {
                new MyLink("Tree map", "/Win/WinTreeMap.xaml"),
                new MyLink("Folders", "/Win/WinTreeView.xaml"),
                new MyLink("Tree list", "/Win/WinTreeList.xaml")
            }});
        }
    }
}
