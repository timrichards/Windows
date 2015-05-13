using FirstFloor.ModernUI.Presentation;
using System;

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
            _mainWindowWR.SetTarget(this);
        }

        static internal LocalUserControlBase CurrentPage
        {
            get { return WithMainWindow(mainWindow => mainWindow._currentPage); }

            set
            {
                WithMainWindow(mainWindow =>
                {
                    if (value == mainWindow._currentPage)
                        return false;

                    mainWindow._currentPage = value;
                    mainWindow.TitleLinks.Remove(_titleLink);

                    if (mainWindow._currentPage is WinProject_MUI)
                        return false;

                    //if (mainWindow._currentPage is info page)
                    //    return false;

                    mainWindow.TitleLinks.Add(_titleLink);
                    return true;
                });
            }
        }

        static internal T WithMainWindow<T>(Func<ModernWindow1, T> doSomethingWith)
        {
            ModernWindow1 mainWindow = null;

            _mainWindowWR.TryGetTarget(out mainWindow);

            if (null == mainWindow)
            {
                MBoxStatic.Assert(99856, false);
                return default(T);
            }

            return doSomethingWith(mainWindow);
        }

        internal void ShowLinks(bool bHidden = false)
        {
            if (bHidden)
            {
                while (MenuLinkGroups.Count > 1)
                    MenuLinkGroups.RemoveAt(1);
            }
            else if (1 == MenuLinkGroups.Count)
            {
                foreach (var group in _links)
                    MenuLinkGroups.Add(group);
            }
        }

        LocalUserControlBase
            _currentPage = null;
        static internal string
            ExtraWindowFakeKey { get { return "/ExtraWindow.xaml"; } }
        static readonly Link
            _titleLink = new Link { DisplayName = "Extra Window", Source = new Uri(ExtraWindowFakeKey, UriKind.Relative) };

        static readonly LinkGroup[]
            _links =
        {
            new LinkGroup { DisplayName="Explore", Links =
            {
                new Link { DisplayName = "Tree map", Source = new Uri("/WinDoubleFile/UtilDirList Local/WinTreeMap_MUI.xaml", UriKind.Relative)},
                new Link { DisplayName = "Folders", Source = new Uri("/WinDoubleFile/WinDoubleFile_Folders_MUI.xaml", UriKind.Relative)},
                new Link { DisplayName = "Tree list", Source = new Uri("/WinDoubleFile/WinDoubleFile_TreeList_MUI.xaml", UriKind.Relative)}
            }},
            new LinkGroup { DisplayName="Files", Links =
            {
                new Link { DisplayName = "Files in folder", Source = new Uri("/WinDoubleFile/WinDoubleFile_Files_MUI.xaml", UriKind.Relative)},
                new Link { DisplayName = "Duplicates", Source = new Uri("/WinDoubleFile/WinDoubleFile_Duplicates_MUI.xaml", UriKind.Relative)}
            }},
            new LinkGroup { DisplayName="Search", Links =
            {
                new Link { DisplayName = "Search", Source = new Uri("/WinDoubleFile/WinDoubleFile_Search_MUI.xaml", UriKind.Relative)}
            }},
            new LinkGroup { DisplayName="Detailed info", Links =
            {
                new Link { DisplayName = "Detailed info", Source = new Uri("/WinDoubleFile/WinDoubleFile_Detail_MUI.xaml", UriKind.Relative)}
            }}
        };

        static readonly WeakReference<ModernWindow1>
            _mainWindowWR = new WeakReference<ModernWindow1>(null);
    }
}
