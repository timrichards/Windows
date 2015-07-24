using System;
using System.Linq;
using System.Windows;

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
                SelectedOne = () => 1 == form_lv.SelectedItems.Count,
                SelectedAny = () => 0 < form_lv.SelectedItems.Count,
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

            if ((winProject._lvProjectVM?.LocalEquals(App.LVprojectVM) ?? false) &&
                OKtoNavigate_UpdateSaveListingsLink(bSaveListings))
            {
                return true;
            }

            Reset();

            if (0 < App.LVprojectVM.Items.Count)
            {
                SaveListingsProcess.Go(App.LVprojectVM);

                if ((0 < App.LVprojectVM.CanLoadCount) &&
                    LocalTV.FactoryCreate(App.LVprojectVM))
                {
                    winProject._lvProjectVM = new LV_ProjectVM(App.LVprojectVM);
                }
            }

            OKtoNavigate_UpdateSaveListingsLink();
            return (null != winProject._lvProjectVM);
        }

        static internal bool OKtoNavigate_UpdateSaveListingsLink(bool bSaveListings = false)
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
                Util.UIthread(99843, () =>
                {
                    foreach (Window window in mainWindow.OwnedWindows)
                        window.Close();
                });
            });

            WinProject winProject = null;

            _weakReference.TryGetTarget(out winProject);
            winProject._lvProjectVM = null;

            LocalTV.WithLocalTV(localTV =>
                localTV.LocalDispose());

            App.FileDictionary.Dispose();
            App.FileDictionary = new FileDictionary();
        }

        LV_ProjectVM
            _lvProjectVM = null;
        static WeakReference<WinProject>
            _weakReference = new WeakReference<WinProject>(null);
    }
}
