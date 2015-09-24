using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class UC_FolderListVM : IDisposable
    {
        protected void Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
            _nicknameUpdater.Clear();           // in case Init() is reused on an existing list: future proof
            _nicknameUpdater.UpdateViewport(UseNicknames);
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99701, TreeSelect_FolderDetailUpdated));
        }

        public virtual void Dispose()
        {
            Util.LocalDispose(_lsDisposable);
        }

        protected virtual void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, int> initiatorTuple) { }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99897, false);      // binding should dim the button
                return;
            }

            _selectedItem.LocalTreeNode.GoToFile(null);
        }

        protected readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99667);
        protected readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
