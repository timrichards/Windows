﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_SinksVM : UC_FolderListVM
    {
        public ICommand Icmd_Unmount { get; private set; }
        internal new UC_SinksVM          // new to hide then call base.Init() and return this
            Init()
        {
            base.Init();
      //      Icmd_Unmount = new RelayCommand(Unmount);

            Util.ThreadMake(() =>
            {
                HideProgressbar();
            });

            return this;
        }

        public override void Dispose()          // calls base.Dispose() which implements IDisposable
        {
            Util.ThreadMake(() =>
            {
                base.Dispose();                 // sets _cts.IsCancellationRequested
            });
        }

        protected override IEnumerable<LVitem_FolderListVM>
            FillList(LocalTreeNode searchFolder)
        {
            _lsFolders = new ConcurrentBag<LVitem_FolderListVM>();
            return _lsFolders;
        }

        protected override void Clear() => _lsFolders = null;

        ConcurrentBag<LVitem_FolderListVM>
            _lsFolders = null;
    }
}
