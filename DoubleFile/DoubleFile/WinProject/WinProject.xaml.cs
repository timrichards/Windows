using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinProject
    {
        public WinProject()
        {
            InitializeComponent();

            form_lv.DataContext =
                App.LVprojectVM =
                new LV_ProjectVM(App.LVprojectVM)
            {
                SelectedOne = () => form_lv.SelectedItems.HasOnlyOne(),
                SelectedAny = () => false == form_lv.SelectedItems.IsEmptyA(),
                Selected = () => form_lv.SelectedItems.Cast<LVitem_ProjectVM>()
            };

            DataContext = new WinProjectVM(App.LVprojectVM);
            _weakReference.SetTarget(this);
            LV_ProjectVM.Modified.Subscribe(x => Reset());
        }

        static internal bool OKtoNavigate()
        {
            if (OKtoNavigate_())
                return true;

            Reset();
            return false;
        }

        static bool OKtoNavigate_()
        {
            if (null == App.LVprojectVM)
                return false;

            WinProject winProjectMUI = null;

            _weakReference.TryGetTarget(out winProjectMUI);

            if ((null != winProjectMUI._lvProjectVM)
                && (App.LVprojectVM.LocalEquals(winProjectMUI._lvProjectVM)))
            {
                if (false == App.LVprojectVM.ItemsCast.Any(lvItem => lvItem.WouldSave))
                    return true;

                if (winProjectMUI._bAskedToSave)
                {
                    if (false == (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                        return true;
                }
                else if (MessageBoxResult.No ==
                    MBoxStatic.ShowDialog("Would you like to scan now? Hit Ctrl while navigating next time to do the scan then.",
                    "One or more listings have not been scanned.",
                    MessageBoxButton.YesNo))
                {
                    winProjectMUI._bAskedToSave = true;
                    return true;
                }
            }

            Reset();

            if (false == App.LVprojectVM.Items.IsEmpty())
            {
                new SaveListingsProcess(App.LVprojectVM);

                if (App.LVprojectVM.ItemsCast.Any(lvItem => lvItem.CanLoad) &&
                    LocalTV.FactoryCreate(App.LVprojectVM))
                {
                    winProjectMUI._lvProjectVM = new LV_ProjectVM(App.LVprojectVM);
                }
            }

            return (null != winProjectMUI._lvProjectVM);
        }

        static void Reset()
        {
            MainWindow.WithMainWindow(mainWindow =>
            {
                Util.UIthread(() =>
                {
                    foreach (Window window in mainWindow.OwnedWindows)
                        window.Close();
                });

                return false;   // from lambda
            });

            WinProject winProjectMUI = null;

            _weakReference.TryGetTarget(out winProjectMUI);
            winProjectMUI._lvProjectVM = null;
            winProjectMUI._bAskedToSave = false;

            if (null != LocalTV.Instance)
                LocalTV.LocalDispose();

            App.FileDictionary.Dispose();
            App.FileDictionary = new FileDictionary();
        }

        bool
            _bAskedToSave = false;
        LV_ProjectVM
            _lvProjectVM = null;
        static WeakReference<WinProject>
            _weakReference = new WeakReference<WinProject>(null);
    }
}
