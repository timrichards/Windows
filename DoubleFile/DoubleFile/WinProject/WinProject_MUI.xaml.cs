using System;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    partial class WinProject_MUI
    {
        public WinProject_MUI()
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

        internal static bool OKtoNavigate()
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

            WinProject_MUI winProjectMUI = null;

            _weakReference.TryGetTarget(out winProjectMUI);

            if ((null != winProjectMUI._lvProjectVM)
                && (App.LVprojectVM.LocalEquals(winProjectMUI._lvProjectVM)))
            {
                return true;
            }

            Reset();

            if (false == App.LVprojectVM.Items.IsEmpty())
            {
                new SaveListingsProcess(App.LVprojectVM);

                if (LocalTV.FactoryCreate(App.LVprojectVM))
                    winProjectMUI._lvProjectVM = new LV_ProjectVM(App.LVprojectVM);
            }

            return (null != winProjectMUI._lvProjectVM);
        }

        static void Reset()
        {
            ModernWindow1.WithMainWindow(mainWindow =>
            {
                foreach (Window window in mainWindow.OwnedWindows)
                    window.Close();

                return false;   // from lambda
            });

            WinProject_MUI winProjectMUI = null;

            _weakReference.TryGetTarget(out winProjectMUI);
            winProjectMUI._lvProjectVM = null;

            if (null != LocalTV.Instance)
                LocalTV.LocalDispose();

            App.FileDictionary.Dispose();
            App.FileDictionary = new FileDictionary();
        }

        LV_ProjectVM
            _lvProjectVM = null;
        static WeakReference<WinProject_MUI>
            _weakReference = new WeakReference<WinProject_MUI>(null);
    }
}
