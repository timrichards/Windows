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
                Statics.LVprojectVM =
                new LV_ProjectVM(Statics.LVprojectVM)
            {
                SelectedOne = () => 1 == form_lv.SelectedItems.Count,
                SelectedAny = () => 0 < form_lv.SelectedItems.Count,
                SelectedItems = () => form_lv.SelectedItems.Cast<LVitem_ProjectVM>()
            };

            DataContext = new WinProjectVM(Statics.LVprojectVM);
            _weakReference.SetTarget(this);
            LV_ProjectVM.Modified.LocalSubscribe(99720, x => Reset());
        }

        static internal bool OKtoNavigate_BuildExplorer(bool bSaveListings)
        {
            if (null == Statics.LVprojectVM)
            {
                Reset();
                return false;
            }

            WinProject winProject = null;

            _weakReference.TryGetTarget(out winProject);

            if ((winProject._lvProjectVM?.LocalEquals(Statics.LVprojectVM) ?? false) &&
                OKtoNavigate_UpdateSaveListingsLink(bSaveListings))
            {
                Util.Assert(99875, false == ReferenceEquals(winProject._lvProjectVM, Statics.LVprojectVM));

                return true;
            }

            Reset();

            if (0 < Statics.LVprojectVM.Items.Count)
            {
                SaveListingsProcess.Go(Statics.LVprojectVM);

                if ((0 < Statics.LVprojectVM.CanLoadCount) &&
                    LocalTV.FactoryCreate(new LV_ProjectVM(Statics.LVprojectVM)))
                {
                    winProject._lvProjectVM = new LV_ProjectVM(Statics.LVprojectVM);
                }
            }

            OKtoNavigate_UpdateSaveListingsLink();
            return (null != winProject._lvProjectVM);
        }

        static internal bool OKtoNavigate_UpdateSaveListingsLink(bool bSaveListings = false)
        {
            var bListingsToSave = Statics.LVprojectVM.ItemsCast.Any(lvItem => lvItem.WouldSave);

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

                    mainWindow.Activate();
                });
            });

            WinProject winProject = null;

            _weakReference.TryGetTarget(out winProject);
            winProject._lvProjectVM = null;

            LocalTV.WithLocalTV(localTV =>
                localTV.LocalDispose());

            Statics.FileDictionary.Dispose();
            Statics.FileDictionary = new FileDictionary();
        }

        LV_ProjectVM
            _lvProjectVM = null;
        static WeakReference<WinProject>
            _weakReference = new WeakReference<WinProject>(null);
    }
}
