﻿using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreeMap.xaml
    /// </summary>
    public partial class WinVolTreeMap
    {
        public WinVolTreeMap()
        {
            InitializeComponent();

            Observable.FromEventPattern(this, "SizeChanged")
                .LocalSubscribe(x => { _ucTreeMap?.ClearSelection(); });

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .LocalSubscribe(x => { _ucTreeMap?.TreeMapVM.LostMouseCapture(); });
        }

        protected override void LocalNavigatedTo()
        {
            do
            {
                string strSourcePath = null;

                new UC_VolumeEditVM
                {
                    FromSourcePathDlg = str => strSourcePath = str,

                    DriveModel_CurrentText = () => null,
                    DriveSerial_CurrentText = () => null,
                    FromListingFileDlg = str => { },
                    FromProbe = (x, y) => { },
                    ListingFile_CurrentText = () => null,
                    SourcePath_CurrentText = () => null,
                }
                    .EditSourcePath();

                Statics.LVprojectVM = new LV_ProjectVM();

                Statics.LVprojectVM.Add(new LVitem_ProjectVM
                {
                    SourcePath = strSourcePath
                },
                    bQuiet: true);

                SaveDirListings.Hash = false;
                SaveListingsProcess.Go(Statics.LVprojectVM);
            }
                while
                (false ==
                ((0 < Statics.LVprojectVM.CanLoadCount) &&
                LocalTV.FactoryCreate(Statics.LVprojectVM)));

            _host.Child =
                _ucTreeMap =
                new UC_TreeMap()
            {
                LocalOwner = Application.Current.MainWindow,
                TreeMapVM = new WinTreeMapVM()
            };

            DataContext = _ucTreeMap.TreeMapVM;

            var treeNode = LocalTV.TopNode;
            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)
                TreeSelect.DoThreadFactory(LocalTV.TopNode, 0);
            else
                treeNode = folderDetail.treeNode;

            _ucTreeMap.TreeMapVM.GoTo(treeNode);
        }

        protected override void LocalNavigatedFrom()
        {
            _ucTreeMap.Dispose();

            _host.Child =
                _ucTreeMap =
                null;

            DataContext = null;
        }

        UC_TreeMap
            _ucTreeMap = null;
    }
}
