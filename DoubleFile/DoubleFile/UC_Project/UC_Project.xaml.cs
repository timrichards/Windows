using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    partial class UC_Project
    {
        public UC_Project()
        {
            InitializeComponent();

            var lvProjectVM =
                new LV_ProjectVM(Statics.WithLVprojectVM(w => w))
            {
                SelectedOne = () => 1 == form_lv.SelectedItems.Count,
                SelectedAny = () => 0 < form_lv.SelectedItems.Count,
                SelectedItems = () => form_lv.SelectedItems.Cast<LVitem_ProjectVM>()
            };

            var winProjectVM = new WinProjectVM(lvProjectVM);

            Observable.FromEventPattern(Application.Current.Dispatcher, "ShutdownStarted")
                .LocalSubscribe(99851, x => winProjectVM.Dispose());

            DataContext = winProjectVM;

            form_lv.DataContext =
                Statics.LVprojectVM =
                lvProjectVM;

            LV_ProjectVM.Modified.LocalSubscribe(99720, x => Reset());
            _weakReference.SetTarget(this);
        }

        static internal bool OKtoNavigate_BuildExplorer(bool bSaveListings)
        {
            if (null == Statics.WithLVprojectVM(w => w))
            {
                Reset();
                return false;
            }

            var winProject = _weakReference.Get(w => w);

            if (Statics.WithLVprojectVM(p => p?.LocalEquals(winProject._vm) ?? false) &&
                OKtoNavigate_UpdateSaveListingsLink(bSaveListings))
            {
                Util.Assert(99875, Statics.WithLVprojectVM(p => false == ReferenceEquals(p, winProject._vm)));
                return true;
            }

            Reset();

            {
                var lvProjectVM = Statics.WithLVprojectVM(w => w);

                if (null == lvProjectVM)
                {
                    Util.Assert(99942, false);

                    Statics.LVprojectVM =
                        lvProjectVM =
                        new LV_ProjectVM();
                }

                if (0 < lvProjectVM.Items.Count)
                    SaveListingsProcess.Go(lvProjectVM);
            }

            var lvProjectVM_Copy = Statics.LVprojectVM_Copy;

            if ((0 < lvProjectVM_Copy.CanLoadCount) &&
                LocalTV.FactoryCreate(lvProjectVM_Copy))
            {
                winProject._vm = lvProjectVM_Copy;
            }

            OKtoNavigate_UpdateSaveListingsLink();
            return (null != winProject._vm);
        }

        static internal bool OKtoNavigate_UpdateSaveListingsLink(bool bSaveListings = false)
        {
            var bListingsToSave = Statics.WithLVprojectVM(p => p?.ItemsCast.Any(lvItem => lvItem.WouldSave) ?? false);

            if (bListingsToSave && bSaveListings)
                bListingsToSave = false;

            MainWindow.UpdateTitleLinks(bListingsToSave);
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

            _weakReference.Get(w =>
                w._vm = null);

            LocalTV.WithLocalTV(localTV =>
                localTV.LocalDispose());

            Statics.FileDictionary.Dispose();
            Statics.FileDictionary = new FileDictionary();
        }

        LV_ProjectVM
            _vm = null;
        static readonly WeakReference<UC_Project>
            _weakReference = new WeakReference<UC_Project>(null);
    }
}
