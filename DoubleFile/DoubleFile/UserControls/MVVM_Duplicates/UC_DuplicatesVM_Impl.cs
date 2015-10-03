using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class UC_DuplicatesVM : IDisposable
    {
        static internal IObservable<Tuple<Tuple<LVitem_ProjectVM, string, string>, decimal>>
            GoToFile => _goToFile;
        static readonly LocalSubject<Tuple<LVitem_ProjectVM, string, string>> _goToFile = new LocalSubject<Tuple<LVitem_ProjectVM, string, string>>();
        static void GoToFileOnNext(Tuple<LVitem_ProjectVM, string, string> value) => _goToFile.LocalOnNext(value, 99848);

        static internal IObservable<Tuple<Tuple<IReadOnlyList<string>, LocalTreeNode>, decimal>>
            UpdateFileDetail => _updateFileDetail;
        static readonly LocalSubject<Tuple<IReadOnlyList<string>, LocalTreeNode>> _updateFileDetail = new LocalSubject<Tuple<IReadOnlyList<string>, LocalTreeNode>>();
        static void UpdateFileDetailOnNext(Tuple<IReadOnlyList<string>, LocalTreeNode> value, decimal nInitiator) => _updateFileDetail.LocalOnNext(value, 99847, nInitiator);

        internal UC_DuplicatesVM
            Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
            _nicknameUpdater.UpdateViewport(UseNicknames);
            _lsDisposable.Add(LV_FilesVM_Base.SelectedFileChanged.Observable.LocalSubscribe(99704, LV_FilesVM_SelectedFileChanged));

            var lastSelectedFile = LV_FilesVM_Base.LastSelectedFile;

            if (null != lastSelectedFile)
                LV_FilesVM_SelectedFileChanged(Tuple.Create(lastSelectedFile, /* UI initiator */ 0m));

            return this;
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposable);
            _nicknameUpdater.Clear();
        }

        void LV_FilesVM_SelectedFileChanged(Tuple<LV_FilesVM_Base.SelectedFileChanged, decimal> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            _treeNode = tuple?.treeNode;
            SelectedItem_Set(null);
            ClearItems();

            if (null == tuple?.lsDupDirFileLines)
                return;

            Util.UIthread(99812, () =>
            {
                foreach (var asFileLine in tuple.lsDupDirFileLines)
                {
                    var lvItemProject_Updater = asFileLine.Item1;

                    if (null == lvItemProject_Updater.ListUpdater)
                        lvItemProject_Updater.ListUpdater = _nicknameUpdater;
                    else    // LV_FilesVM shouldn't be using this ListUpdater
                        Util.Assert(99785, lvItemProject_Updater.ListUpdater == _nicknameUpdater);

                    Add(new LVitem_FileDuplicatesVM(new[] { asFileLine.Item2[0] })
                    {
                        FileLine =
                            asFileLine.Item2
                            .Skip(1)                    // makes this an LV line: knColLengthLV
                            .ToArray(),

                        LVitemProject_Updater = lvItemProject_Updater
                    });
                }
            });
        }

        internal void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99901, false);    // binding should dim the button
                return;
            }

            GoToFileOnNext(Tuple.Create((LVitem_ProjectVM)_selectedItem.LVitemProject_Updater, _selectedItem.SubItems[0], _selectedItem.Filename));
        }

        readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99683);
        LocalTreeNode
            _treeNode = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
