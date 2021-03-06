﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_Project
    {
        public UC_Project()
        {
            InitializeComponent();

            _weakReference.Get(w =>
            {
                DataContext = w.DataContext;
                form_lv.DataContext = w.form_lv.DataContext;                                        // A
                Util.UIthread(99965, CommandManager.InvalidateRequerySuggested);
            });

            if (null == DataContext)
            {
                var lvProjectVM =
                    new LV_ProjectVM(Statics.WithLVprojectVM(w => w))
                {
                    SelectedOne = () => 1 == form_lv.SelectedItems.Count,
                    SelectedAny = () => 0 < form_lv.SelectedItems.Count,
                    SelectedItems = () => form_lv.SelectedItems.Cast<LVitem_ProjectVM>()
                };

                var winProjectVM = new UC_ProjectVM(lvProjectVM);

                Observable.FromEventPattern(Application.Current.Dispatcher, "ShutdownStarted")      // TODO: add this to A
                    .LocalSubscribe(99851, x => winProjectVM.Dispose());

                DataContext = winProjectVM;

                form_lv.DataContext =
                    Statics.LVprojectVM =
                    lvProjectVM;
            }

            LV_ProjectVM.Modified_Called.LocalSubscribe(99720, x => Reset());
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
                    SaveDirListings.Go(lvProjectVM);
            }

            LV_ProjectVM lvProjectVM_Copy = null;

            try
            {
                lvProjectVM_Copy = Statics.LVprojectVM_Copy;
            }
            catch (LVitem_ProjectVM.InvalidNicknamePathCharException)
            {
                MBoxStatic.ShowOverlay("Invalid path character exception in Nickname of one of the paths. Can't contain any of " +
                    LVitem_ProjectVM.InvalidNicknamePathCharException.test, "Navigate and Build Explorer");

                return false;
            }

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
                    mainWindow.Title = Util.Localized("Title");

                    foreach (Window window in mainWindow.OwnedWindows)
                        window.Close();

                    mainWindow.Activate();
                });
            });

            _weakReference.Get(w =>
                w._vm = null);

            LocalTV.WithLocalTV(localTV =>
                localTV.LocalDispose());

            Statics.DupeFileDictionary.Dispose();
            Statics.DupeFileDictionary = new DupeFileDictionary();
        }

        LV_ProjectVM
            _vm = null;
        static readonly WeakReference<UC_Project>
            _weakReference = new WeakReference<UC_Project>(null);
    }
}
