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

            MenuLinkGroups.Add(new LinkGroup { DisplayName="Explore", Links =
            {
                new MyLink("Tree map", "/DoubleFile;component/Win/WinTreeMap.xaml"),
                new MyLink("Folders", "/DoubleFile;component/Win/WinTreeView.xaml"),
                new MyLink("Tree list", "/DoubleFile;component/Win/WinTreeList.xaml")
            }});

            TitleLinks.Add(_extraWindowLink);
        }

        internal const string ExtraWindowFakeKey = "/ExtraWindow.xaml";
        static readonly Link _extraWindowLink = new MyLink("Extra window", ExtraWindowFakeKey);
    }
}
