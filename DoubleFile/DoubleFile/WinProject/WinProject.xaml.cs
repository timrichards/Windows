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

        static internal bool OKtoNavigate_BuildExplorer(bool bSaveListings)
        {
            if (null == App.LVprojectVM)
            {
                Reset();
                return false;
            }

            WinProject winProject = null;

            _weakReference.TryGetTarget(out winProject);

            if ((null != winProject._lvProjectVM)
                && App.LVprojectVM.LocalEquals(winProject._lvProjectVM)
                && OKtoNavigate_UpdateSaveListingsLink(winProject, bSaveListings))
            {
                return true;
            }

            Reset();

            if (false == App.LVprojectVM.Items.IsEmpty())
            {
                SaveListingsProcess.Go(App.LVprojectVM);

                if (App.LVprojectVM.ItemsCast.Any(lvItem => lvItem.CanLoad) &&
                    LocalTV.FactoryCreate(App.LVprojectVM))
                {
                    winProject._lvProjectVM = new LV_ProjectVM(App.LVprojectVM);
                }
            }

            OKtoNavigate_UpdateSaveListingsLink(winProject);
            return (null != winProject._lvProjectVM);
        }

        static bool OKtoNavigate_UpdateSaveListingsLink(WinProject winProject, bool bSaveListings = false)
        {
            var bListingsToSave = App.LVprojectVM.ItemsCast.Any(lvItem => lvItem.WouldSave);

            if (bListingsToSave && bSaveListings)
                bListingsToSave = false;

            MainWindow.WithMainWindowA(mainWindow =>
                mainWindow.UpdateTitleLinks(bListingsToSave));

            return (false == bSaveListings);
        }

        static void Reset()
        {
            MainWindow.WithMainWindowA(mainWindow =>
            {
                Util.UIthread(() =>
                {
                    foreach (Window window in mainWindow.OwnedWindows)
                        window.Close();
                });
            });

            WinProject winProject = null;

            _weakReference.TryGetTarget(out winProject);
            winProject._lvProjectVM = null;

            if (null != LocalTV.Instance)
                LocalTV.LocalDispose();

            App.FileDictionary.Dispose();
            App.FileDictionary = new FileDictionary();
        }

        LV_ProjectVM
            _lvProjectVM = null;
        static WeakReference<WinProject>
            _weakReference = new WeakReference<WinProject>(null);
    }
}
