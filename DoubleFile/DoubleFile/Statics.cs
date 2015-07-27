using System.Windows.Media;
using System;
using System.Windows;

namespace DoubleFile
{
    public class Statics
    {
        static internal LocalUserControlBase
            CurrentPage
        {
            get { return _wr.Get(s => s._currentPage); }
            set
            {
                _wr.Get(s =>
                {
                    if (value == s._currentPage)
                        return s;   // from lambda

                    s._currentPage = value;
                    MainWindow.WithMainWindowA(mainWindow => mainWindow.UpdateTitleLinks());
                    return s;       // from lambda
                });
            }
        }
        LocalUserControlBase _currentPage;

        static internal LV_ProjectVM
            LVprojectVM { get { return _wr.Get(s => s._lvProjectVM); } set { _wr.Get(s => s._lvProjectVM = value); } }
        LV_ProjectVM _lvProjectVM;

        static internal FileDictionary
            FileDictionary { get { return _wr.Get(s => s._fileDictionary); } set { _wr.Get(s => s._fileDictionary = value); } }
        FileDictionary _fileDictionary = new FileDictionary();

        static internal SaveDirListings
            SaveDirListings { get { return _wr.Get(s => s._saveDirListings); } set { _wr.Get(s => s._saveDirListings = value); } }
        SaveDirListings _saveDirListings;

        static internal ILocalWindow
            TopWindow
        {
            get { return _wr.Get(s => s._topWindow); }
            set { _wr.Get(s => s._topWindow = Util.AssertNotNull(99775, value)); }
        }
        ILocalWindow _topWindow;

        static public bool
            AppActivated { get { return _wr.Get(s => s._appActivated); } set { _wr.Get(s => s._appActivated = value); } }
        bool _appActivated;

        static public ImageSource
            Icon { get { return _wr.Get(s => s._icon); } set { _wr.Get(s => s._icon = value); } }
        ImageSource _icon;

        public Statics()
        {
            if (_wr.Get(s => true))
            {
                Util.Assert(99776, false);
                return;
            }

            _wr.SetTarget(this);

            foreach (var strSource in new[]
            {
                "/FirstFloor.ModernUI;component/Assets/ModernUI.xaml",
                "/DoubleFile;component/Assets/LocalBlankWindow.xaml",
                "/DoubleFile;component/Assets/LocalMUIdark.xaml",
                "/DoubleFile;component/Assets/Strings.xaml",
                "/DoubleFile;component/Assets/LocalStyles.xaml"
            })
                Application.Current?.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(strSource, UriKind.Relative)
            });
        }

        static WeakReference<Statics> _wr = new WeakReference<Statics>(null);

        // SimulatingModal flag

        // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
        // properly focus: a second click is needed to move the window or do anything in it.

        // 3/26/15 This is true because e.g. 1) left open or cancel during Explorer initialize:
        // the Folders VM disappears and crashes on close. 2) Do you want to cancel, left open,
        // mysteriously leaves WinProject unpopulated after clicking OK: does not run any code
        // in MainWindow.xaml.cs after volumes.ShowDialog. Acts like a suppressed null pointer.

        // 6/4/15 This is also true because of GoModeless(), which might also be possible via subclass:
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644996%28v=vs.85%29.aspx#modeless_box
        internal const bool SimulatingModal = true;   // Change it here to switch to simulated dialog
    }
}
