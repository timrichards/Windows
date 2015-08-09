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

            var lvProjectVM =
                new LV_ProjectVM(Statics.WithLVprojectVM(w => w))
            {
                SelectedOne = () => 1 == form_lv.SelectedItems.Count,
                SelectedAny = () => 0 < form_lv.SelectedItems.Count,
                SelectedItems = () => form_lv.SelectedItems.Cast<LVitem_ProjectVM>()
            };

            DataContext = new WinProjectVM(lvProjectVM);
            _weakReference.SetTarget(this);

            form_lv.DataContext =
                Statics.LVprojectVM =
                lvProjectVM;

            LV_ProjectVM.Modified.LocalSubscribe(99720, x => Reset());
        }

        static internal bool OKtoNavigate_BuildExplorer(bool bSaveListings)
        {
            if (null == Statics.WithLVprojectVM(w => w))
            {
                Reset();
                return false;
            }

            WinProject winProject = null;

            _weakReference.TryGetTarget(out winProject);

            if (Statics.WithLVprojectVM(p => p?.LocalEquals(winProject._lvProjectVM) ?? false) &&
                OKtoNavigate_UpdateSaveListingsLink(bSaveListings))
            {
                Util.Assert(99875, Statics.WithLVprojectVM(p => false == ReferenceEquals(p, winProject._lvProjectVM)));
                return true;
            }

            Reset();

            var lvProjectVM = Statics.WithLVprojectVM(w => w);

            if (0 < (lvProjectVM?.Items.Count ?? 0))
            {
                SaveListingsProcess.Go(lvProjectVM);

                var lvProjectVM_Copy = Statics.LVprojectVM_Copy;

                if ((0 < lvProjectVM.CanLoadCount) &&
                    LocalTV.FactoryCreate(lvProjectVM_Copy))
                {
                    winProject._lvProjectVM = lvProjectVM_Copy;
                }
            }

            OKtoNavigate_UpdateSaveListingsLink();
            return (null != winProject._lvProjectVM);
        }

        static internal bool OKtoNavigate_UpdateSaveListingsLink(bool bSaveListings = false)
        {
            var bListingsToSave = Statics.WithLVprojectVM(p => p?.ItemsCast.Any(lvItem => lvItem.WouldSave) ?? false);

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
