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

        static T WithMainWindow<T>(Func<ModernWindow1, T> doSomethingWith)
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

        static internal string
            ExtraWindowFakeKey { get { return "/ExtraWindow.xaml"; } }
        LocalUserControlBase
            _currentPage = null;
        static readonly Link
            _titleLink = new Link() { DisplayName = "Extra Window", Source = new Uri(ExtraWindowFakeKey, UriKind.Relative) };
        static readonly WeakReference<ModernWindow1>
            _mainWindowWR = new WeakReference<ModernWindow1>(null);
    }
}
