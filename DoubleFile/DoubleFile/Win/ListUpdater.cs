using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    interface IListUpdater
    {
        void RaiseListUpdate();
    }

    // can't be struct because of pass-by-reference
    class ListUpdater<T>                                   // One per search or folder window
    {
        internal T Value { get; private set; }

        internal void LastGet(IListUpdater lvItemVM)
        {
            if (false == _bUpdating)
                _lastGets.Add(lvItemVM);
        }

        internal void UpdateViewport(T value)
        {
            Value = value;
            _lastGets = _lastGets.Skip(Math.Max(0, _lastGets.Count - 1024)).ToList();
            _bUpdating = true;

            foreach (var lvitemVM in _lastGets)
                lvitemVM.RaiseListUpdate();

            _bUpdating = false;
        }

        internal void Clear() =>
            _lastGets = new List<IListUpdater> { };

        List<IListUpdater>
            _lastGets = new List<IListUpdater> { };
        bool
            _bUpdating = false;
    }
}
