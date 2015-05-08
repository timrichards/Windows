using System;
using System.Linq;

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
        }

        internal static bool InitExplorer()
        {
            {
                LV_ProjectVM lvProjectVM = null;

                _lvProjectVM_WR.TryGetTarget(out lvProjectVM);

                if ((null == App.LVprojectVM) ||
                    (App.LVprojectVM == lvProjectVM) ||
                    (App.LVprojectVM.LocalEquals(lvProjectVM)))
                {
                    if (_bCreated)
                        return false;
                }
            }

            _bCreated = false;
            _lvProjectVM_WR.SetTarget(App.LVprojectVM);

            if (null != LocalTV.Instance)
                LocalTV.LocalDispose();

            App.FileDictionary.Dispose();
            App.FileDictionary = new FileDictionary();

            if (App.LVprojectVM.Items.IsEmpty())
                return false;

            LocalTV.FactoryCreate(App.LVprojectVM);
            _bCreated = true;
            return true;
        }

        static WeakReference<LV_ProjectVM>
            _lvProjectVM_WR = new WeakReference<LV_ProjectVM>(null);
        static bool
            _bCreated = false;
    }
}
