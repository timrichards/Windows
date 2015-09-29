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
        public static readonly DependencyProperty GoofballXProperty =
            DependencyProperty.Register("GoofballX", typeof(double), typeof(UC_TreeMap));

        public static readonly DependencyProperty GoofballYProperty =
            DependencyProperty.Register("GoofballY", typeof(double), typeof(UC_TreeMap));

        public UC_TreeMap()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;
        }

        protected override void LocalNavigatedTo()
        {
            _lsDisposable.Add(Observable.FromEventPattern(form_Slider, "LostMouseCapture")
                .LocalSubscribe(99682, x => _vm?.LostMouseCapture()));

            DataContext =
                _vm =
                new UC_TreeMapVM
            {
                LocalOwner = LocalOwner
            };

            _lsDisposable.Add(_vm);

            var bMouseDown = false;

            _lsDisposable.Add(Observable.FromEventPattern(this, "MouseDown")
                .LocalSubscribe(99698, x => bMouseDown = true));

            _lsDisposable.Add(Observable.FromEventPattern<MouseButtonEventArgs>(this, "MouseUp")
                .LocalSubscribe(99697, args =>
            {
                if (bMouseDown)
                {
                    var pt = args.EventArgs.GetPosition(form_Canvas);

                    pt.X /= form_Canvas.ActualWidth;
                    pt.Y /= form_Canvas.ActualHeight;
                    _vm.MouseUp(pt);
                }

                bMouseDown = false;
            }));

            _lsDisposable.Add(Observable.FromEventPattern(form_rectGoofball, "LayoutUpdated")
                .LocalSubscribe(99625, args =>
            {
                var pt = new Point(form_rectGoofball.ActualWidth, form_rectGoofball.ActualHeight);

                pt = form_rectGoofball.TransformToAncestor(this).Transform(pt);
                SetValue(GoofballXProperty, pt.X - form_Goofball.ActualWidth / 2);
                SetValue(GoofballYProperty, pt.Y - form_Goofball.ActualHeight / 2);
            }));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)
            {
                TreeSelect.DoThreadFactory(LocalTV.TopNode, 0);
            }
            else
            {
                _vm.GoTo(folderDetail.treeNode);
                _vm.TreeNode = folderDetail.treeNode;
            }
        }

        protected override void LocalNavigatedFrom()
        {
            var treeNode = _vm?.TreeNode;

            DataContext =
                _vm = null;

            Util.LocalDispose(_lsDisposable);
            _lsDisposable.Clear();

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if ((null != folderDetail) &&
                (treeNode != folderDetail.treeNode))
            {
                TreeSelect.DoThreadFactory(treeNode, 0);
            }
        }

        UC_TreeMapVM
            _vm = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
