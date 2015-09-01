﻿using System;
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
            CantDupeThisUsercontrol = true;
        }

        protected override void LocalNavigatedTo()
        {
            _lsDisposable.Add(Observable.FromEventPattern(form_Slider, "LostMouseCapture")
                .LocalSubscribe(99682, x => _ucTreeMap?.LostMouseCapture()));

            DataContext =
                _ucTreeMap =
                new UC_TreeMapVM
            {
                LocalOwner = LocalOwner
            };

            _lsDisposable.Add(_ucTreeMap);

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
                    _ucTreeMap.form_tmapUserCtl_MouseUp(pt);
                }

                bMouseDown = false;
            }));

            Observable.FromEventPattern<SizeChangedEventArgs>(this, "SizeChanged")
                .LocalSubscribe(99806, x =>
            {

            });

            var treeNode = LocalTV.TopNode;
            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)
            {
                TreeSelect.DoThreadFactory(LocalTV.TopNode, 0);
            }
            else
            {
                treeNode = folderDetail.treeNode;
                _ucTreeMap.GoTo(treeNode);
            }
        }

        protected override void LocalNavigatedFrom()
        {
            var treeNode = _ucTreeMap.TreeNode;

            DataContext = 
                _ucTreeMap = null;

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
            _ucTreeMap = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
