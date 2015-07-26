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

            var W = "/DoubleFile;component/Win";

            MenuLinkGroups.Add(new LinkGroup { DisplayName="Explore", Links =
            {
                new MyLink("Tree map", W + "/WinTreeMap.xaml"),
                new MyLink("Folders", W + "/WinTreeView.xaml"),
                new MyLink("Tree list", W + "/WinTreeList.xaml")
            }});

            MenuLinkGroups.Add(new LinkGroup { DisplayName = "Files", Links =
            {
                new MyLink("Files in folder", W + "/WinFiles.xaml"),
            }});

            MenuLinkGroups.Add(new LinkGroup { DisplayName="Search", Links =
            {
                new MyLink("Search", W + "/WinSearch.xaml")
            }});

            MenuLinkGroups.Add(new LinkGroup { DisplayName = "Detailed info", Links =
            {
                new MyLink("Detailed info", W + "/WinDetail.xaml")
            }});

            TitleLinks.Add(DoubleFile.MainWindow._extraWindowLink);
        }
    }
}
