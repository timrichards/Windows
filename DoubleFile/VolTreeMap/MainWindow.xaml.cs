using DoubleFile;
using FirstFloor.ModernUI.Presentation;
using System;
using System.Windows;

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
                Source = new Uri(strSource, UriKind.Relative);
            }
        }

        static Action Init = null;
        static void InitForMainWindowOnly(Action init) => Init = init;
        public
            MainWindow()
            : base(InitForMainWindowOnly)
        {
            Statics.Icon = Icon;
            Init();
            Init = null;
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            var W = "/DoubleFile;component/UserControls";

            MenuLinkGroups.Add(new LinkGroup { DisplayName="Trees", Links =
            {
                new MyLink("Tree map", W + "/UC_TreeMap.xaml"),
                new MyLink("Tree view", W + "/UC_TreeView.xaml"),
                new MyLink("Tree list", W + "/UC_TreeList.xaml")
            }});

            MenuLinkGroups.Add(new LinkGroup { DisplayName="Search", Links =
            {
                new MyLink("Search", W + "/UC_Search.xaml")
            }});

            MenuLinkGroups.Add(new LinkGroup { DisplayName = "Detailed info", Links =
            {
                new MyLink("Detailed info", W + "/UC_Detail.xaml"),
                new MyLink("Files here", W + "/UC_Files.xaml"),
            }});

            TitleLinks.Add(DoubleFile.MainWindow._extraWindowLink);
        }
    }
}
