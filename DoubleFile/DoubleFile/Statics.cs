using System.Windows.Media;
using System;

namespace DoubleFile
{
    class Statics
    {
        static internal LV_ProjectVM
            LVprojectVM { get { return With(s => s._lvProjectVM); } set { With(s => s._lvProjectVM = value); } }
        LV_ProjectVM _lvProjectVM;

        static internal FileDictionary
            FileDictionary { get { return With(s => s._fileDictionary); } set { With(s => s._fileDictionary = value); } }
        FileDictionary _fileDictionary = new FileDictionary();

        static internal SaveDirListings
            SaveDirListings { get { return With(s => s._saveDirListings); } set { With(s => s._saveDirListings = value); } }
        SaveDirListings _saveDirListings;

        static internal ILocalWindow
            TopWindow { get { return With(s => s._topWindow); } set { With(s => s._topWindow = value); } }
        ILocalWindow _topWindow;

        static internal bool
            AppActivated { get { return With(s => s._appActivated); } set { With(s => s._appActivated = value); } }
        bool _appActivated;

        static internal ImageSource
            Icon { get { return With(s => s._icon); } set { With(s => s._icon = value); } }
        ImageSource _icon;

        internal Statics()
        {
            if (With(s =>
            {
                MBoxStatic.Assert(99776, false);
                return true;    // from lambda
            }))
            {
                return;
            }

            _wr.SetTarget(this);
        }

        static T With<T>(Func<Statics, T> doSomethingWith)
        {
            Statics statics = null;

            _wr.TryGetTarget(out statics);

            return
                (null != statics)
                ? doSomethingWith(statics)
                : default(T);
        }

        static WeakReference<Statics>
            _wr = new WeakReference<Statics>(null);

        // SimulatingModal flag

        // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
        // properly focus: a second click is needed to move the window or do anything in it.

        // 3/26/15 This is true because e.g. 1) left open or cancel during Explorer initialize:
        // the Folders VM disappears and crashes on close. 2) Do you want to cancel, left open,
        // mysteriously leaves WinProject unpopulated after clicking OK: does not run any code
        // in MainWindow.xaml.cs after volumes.ShowDialog. Acts like a suppressed null pointer.

        // 6/4/15 This is also true because of GoModeless(), which I've never seen demostrated
        // natively, and believe is not possible.
        internal const bool SimulatingModal = true;   // Change it here to switch to simulated dialog
    }
}
