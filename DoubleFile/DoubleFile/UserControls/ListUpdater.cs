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
    class ListUpdater<T>                                    // One for each list
    {
        internal T Value { get; private set; }

        internal ListUpdater(Decimal nAssertLoc)
        {
            _nAssertLoc = nAssertLoc;
        }

        internal void LastGet(IListUpdater lastGet)
        {
            if (false == _bUpdating)
                _lastGets.Add(lastGet);
        }

        internal void UpdateViewport(T value)
        {
            Value = value;
            _lastGets = _lastGets.Skip(Math.Max(0, _lastGets.Count - 1024)).ToList();
            _bUpdating = true;

            foreach (var lastGet in _lastGets)
                lastGet.RaiseListUpdate();

            _bUpdating = false;
        }

        internal void Clear() =>
            _lastGets = new List<IListUpdater>();

        List<IListUpdater>
            _lastGets = new List<IListUpdater>();
        bool
            _bUpdating = false;
        Decimal
            _nAssertLoc = -1;
    }
}
