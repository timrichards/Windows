using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreeMap.xaml
    /// </summary>
    public partial class UC_TreeMap
    {
        public UC_TreeMap()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .LocalSubscribe(99682, x => _ucTreeMap?.LostMouseCapture());

            CantDupeThisUsercontrol = true;
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _ucTreeMap =
                new WinTreeMapVM
            {
                LocalOwner = Application.Current.MainWindow,
            };

            _lsDisposable.Add(_ucTreeMap);

            var bMouseDown = false;

            _lsDisposable.Add(Observable.FromEventPattern(this, "MouseDown")
                .LocalSubscribe(99698, x => bMouseDown = true));

            _lsDisposable.Add(Observable.FromEventPattern<MouseButtonEventArgs>(this, "MouseUp")
                .LocalSubscribe(99697, args =>
            {
                if (bMouseDown)
                    _ucTreeMap.form_tmapUserCtl_MouseUp(args.EventArgs.GetPosition(this));

                bMouseDown = false;
            }));

            Observable.FromEventPattern<SizeChangedEventArgs>(this, "SizeChanged")
                .LocalSubscribe(99806, x =>
            {

            });

            var treeNode = LocalTV.TopNode;
            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)
                TreeSelect.DoThreadFactory(LocalTV.TopNode, 0);
            else
                treeNode = folderDetail.treeNode;

            _ucTreeMap.GoTo(treeNode);
        }

        protected override void LocalNavigatedFrom()
        {
            Util.LocalDispose(_lsDisposable);

            DataContext = 
                _ucTreeMap = null;
        }

        WinTreeMapVM
            _ucTreeMap = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
